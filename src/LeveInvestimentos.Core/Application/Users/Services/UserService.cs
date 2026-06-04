using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Application.Users.Commands;
using LeveInvestimentos.Core.Application.Users.DTOs;
using LeveInvestimentos.Core.Domain.Entities;
using LeveInvestimentos.Core.Domain.Enums;
using LeveInvestimentos.Core.Domain.ValueObjects;

namespace LeveInvestimentos.Core.Application.Users.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IEmailOutbox _emailOutbox;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordGenerator passwordGenerator,
        IEmailOutbox emailOutbox)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordGenerator = passwordGenerator;
        _emailOutbox = emailOutbox;
    }

    public async Task<Result<UserDetailsDto>> CreateAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        User user;
        try
        {
            user = User.Create(
                command.FullName,
                command.BirthDate,
                command.Email,
                new Address(
                    command.Street,
                    command.Number,
                    command.City,
                    command.State),
                command.LandlinePhone,
                command.MobilePhone,
                command.ProfilePhotoPath,
                command.Role,
                DateOnly.FromDateTime(DateTime.UtcNow),
                command.ManagerId);
        }
        catch (ArgumentException ex)
        {
            return Result<UserDetailsDto>.Failure(new Error("Users.InvalidUserData", ex.Message));
        }

        if (await _userRepository.ExistsByEmailAsync(user.Email!, cancellationToken))
        {
            return Result<UserDetailsDto>.Failure(new Error("Users.EmailAlreadyExists", "E-mail is already registered."));
        }

        if (user.Role == UserRole.Subordinate)
        {
            var manager = await _userRepository.GetByIdAsync(user.ManagerId!.Value, cancellationToken);
            if (manager is null || manager.Role != UserRole.Manager)
            {
                return Result<UserDetailsDto>.Failure(new Error("Users.InvalidManager", "Manager was not found."));
            }
        }

        var password = _passwordGenerator.Generate();

        await _userRepository.AddAsync(user, password, cancellationToken);
        await SendCredentialsEmailAsync(user, password, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserDetailsDto>.Success(MapDetails(user));
    }

    public async Task<Result<UserDetailsDto>> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return Result<UserDetailsDto>.Failure(new Error("Users.IdRequired", "User id is required."));
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user is null
            ? Result<UserDetailsDto>.Failure(new Error("Users.NotFound", "User was not found."))
            : Result<UserDetailsDto>.Success(MapDetails(user));
    }

    public async Task<Result<IReadOnlyCollection<UserListItemDto>>> ListSubordinatesAsync(
        Guid managerId,
        CancellationToken cancellationToken = default)
    {
        if (managerId == Guid.Empty)
        {
            return Result<IReadOnlyCollection<UserListItemDto>>.Failure(new Error("Users.ManagerRequired", "Manager id is required."));
        }

        var users = await _userRepository.ListByManagerIdAsync(managerId, cancellationToken);
        return Result<IReadOnlyCollection<UserListItemDto>>.Success(users.Select(MapListItem).ToArray());
    }

    public async Task<Result<IReadOnlyCollection<UserListItemDto>>> ListManagedUsersAsync(
        Guid managerId,
        CancellationToken cancellationToken = default)
    {
        if (managerId == Guid.Empty)
        {
            return Result<IReadOnlyCollection<UserListItemDto>>.Failure(new Error("Users.ManagerRequired", "Manager id is required."));
        }

        var users = await _userRepository.ListManagedUsersAsync(managerId, cancellationToken);
        return Result<IReadOnlyCollection<UserListItemDto>>.Success(users.Select(MapListItem).ToArray());
    }

    private Task SendCredentialsEmailAsync(User user, string password, CancellationToken cancellationToken)
    {
        var body = $"Acesso criado para {user.FullName}. E-mail: {user.Email}. Senha temporaria: {password}. Troque a senha no primeiro login.";
        return _emailOutbox.EnqueueAsync(user.Email!, "Credenciais de acesso", body, cancellationToken);
    }

    private static UserListItemDto MapListItem(User user)
    {
        return new UserListItemDto(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            user.Role,
            user.ManagerId,
            user.Manager?.FullName,
            user.MustChangePassword);
    }

    private static UserDetailsDto MapDetails(User user)
    {
        return new UserDetailsDto(
            user.Id,
            user.FullName,
            user.BirthDate,
            user.Email ?? string.Empty,
            user.Role,
            user.ManagerId,
            user.Manager?.FullName,
            user.Address.Street,
            user.Address.Number,
            user.Address.City,
            user.Address.State,
            user.LandlinePhone.Value,
            user.MobilePhone.Value,
            user.ProfilePhotoPath,
            user.MustChangePassword);
    }
}
