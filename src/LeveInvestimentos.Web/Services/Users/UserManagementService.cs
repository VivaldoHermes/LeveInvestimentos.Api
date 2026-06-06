using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Application.Users.Commands;
using LeveInvestimentos.Core.Application.Users.DTOs;
using LeveInvestimentos.Core.Application.Users.Services;
using LeveInvestimentos.Core.Domain.Enums;
using LeveInvestimentos.Web.ViewModels.Users;

namespace LeveInvestimentos.Web.Services.Users;

public sealed class UserManagementService : IUserManagementService
{
    private const string InvalidProfilePhotoCode = "Users.InvalidProfilePhoto";

    private readonly IUserService _userService;
    private readonly IFileStorage _fileStorage;

    public UserManagementService(IUserService userService, IFileStorage fileStorage)
    {
        _userService = userService;
        _fileStorage = fileStorage;
    }

    public async Task<Result<IReadOnlyCollection<UserListItemViewModel>>> ListManagedUsersAsync(
        Guid managerId,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.ListManagedUsersAsync(managerId, cancellationToken);
        return result.IsFailure
            ? Result<IReadOnlyCollection<UserListItemViewModel>>.Failure(result.Error)
            : Result<IReadOnlyCollection<UserListItemViewModel>>.Success(result.Value.Select(MapListItem).ToArray());
    }

    public async Task<Result<UserDetailsDto>> CreateAsync(
        Guid managerId,
        CreateUserViewModel model,
        CancellationToken cancellationToken = default)
    {
        var profilePhotoResult = await SaveProfilePhotoAsync(model, cancellationToken);
        if (profilePhotoResult.IsFailure)
        {
            return Result<UserDetailsDto>.Failure(profilePhotoResult.Error);
        }

        return await _userService.CreateAsync(
            ToCommand(managerId, model, profilePhotoResult.Value),
            cancellationToken);
    }

    private async Task<Result<string>> SaveProfilePhotoAsync(
        CreateUserViewModel model,
        CancellationToken cancellationToken)
    {
        if (model.ProfilePhoto is not { Length: > 0 } profilePhoto)
        {
            return Result<string>.Success(string.Empty);
        }

        try
        {
            await using var content = profilePhoto.OpenReadStream();
            var storedFile = await _fileStorage.SaveAsync(
                content,
                profilePhoto.FileName,
                profilePhoto.ContentType,
                cancellationToken);

            return Result<string>.Success(storedFile.StorageKey);
        }
        catch (InvalidOperationException ex)
        {
            return Result<string>.Failure(CreateInvalidProfilePhotoError(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return Result<string>.Failure(CreateInvalidProfilePhotoError(ex.Message));
        }
    }

    private static Error CreateInvalidProfilePhotoError(string message)
    {
        return new Error(
            InvalidProfilePhotoCode,
            string.IsNullOrWhiteSpace(message) ? "A foto enviada nao e valida." : message);
    }

    private static CreateUserCommand ToCommand(
        Guid managerId,
        CreateUserViewModel model,
        string profilePhotoStorageKey)
    {
        return new CreateUserCommand(
            model.FullName,
            model.BirthDate!.Value,
            model.Email,
            model.Street,
            model.Number,
            model.City,
            model.State,
            model.LandlinePhone,
            model.MobilePhone,
            profilePhotoStorageKey,
            model.Role == UserRole.Subordinate ? managerId : null,
            model.Role);
    }

    private static UserListItemViewModel MapListItem(UserListItemDto user)
    {
        return new UserListItemViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            ManagerName = user.ManagerName,
            MustChangePassword = user.MustChangePassword
        };
    }
}
