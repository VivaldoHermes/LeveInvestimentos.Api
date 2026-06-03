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
    private readonly IEmailSender _emailSender;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordGenerator passwordGenerator,
        IEmailSender emailSender)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordGenerator = passwordGenerator;
        _emailSender = emailSender;
    }

    public async Task<Result<UserDetailsDto>> CreateAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = CreateUserCommandValidator.Validate(command, DateOnly.FromDateTime(DateTime.UtcNow));
        if (validation.IsFailure)
        {
            return Result<UserDetailsDto>.Failure(validation.Error);
        }

        var normalizedEmail = command.Email.Trim();
        if (await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            return Result<UserDetailsDto>.Failure(new Error("Users.EmailAlreadyExists", "E-mail is already registered."));
        }

        if (command.Role == UserRole.Subordinate)
        {
            var manager = await _userRepository.GetByIdAsync(command.ManagerId!.Value, cancellationToken);
            if (manager is null || manager.Role != UserRole.Manager)
            {
                return Result<UserDetailsDto>.Failure(new Error("Users.InvalidManager", "Manager was not found."));
            }
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FullName = command.FullName.Trim(),
            BirthDate = command.BirthDate,
            Address = new Address(
                command.Street,
                command.Number,
                command.Complement,
                command.Neighborhood,
                command.City,
                command.State,
                command.ZipCode),
            LandlinePhone = string.IsNullOrWhiteSpace(command.LandlinePhone)
                ? null
                : new PhoneNumber(command.LandlinePhone),
            MobilePhone = new PhoneNumber(command.MobilePhone),
            ProfilePhotoPath = string.IsNullOrWhiteSpace(command.ProfilePhotoPath) ? null : command.ProfilePhotoPath.Trim(),
            ManagerId = command.Role == UserRole.Subordinate ? command.ManagerId : null,
            Role = command.Role,
            MustChangePassword = true
        };

        var password = _passwordGenerator.Generate();

        await _userRepository.AddAsync(user, password, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await SendCredentialsEmailAsync(user, password, cancellationToken);

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
        return _emailSender.SendAsync(user.Email!, "Credenciais de acesso", body, cancellationToken);
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
            user.Address?.Street ?? string.Empty,
            user.Address?.Number ?? string.Empty,
            user.Address?.Complement ?? string.Empty,
            user.Address?.Neighborhood ?? string.Empty,
            user.Address?.City ?? string.Empty,
            user.Address?.State ?? string.Empty,
            user.Address?.ZipCode ?? string.Empty,
            user.LandlinePhone?.Value,
            user.MobilePhone?.Value ?? string.Empty,
            user.ProfilePhotoPath,
            user.MustChangePassword);
    }
}
