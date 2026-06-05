using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Core.Application.Account;
using LeveInvestimentos.Core.Application.Account.DTOs;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LeveInvestimentos.Infrastructure.Identity;

public sealed class AccountIdentityGateway : IAccountIdentityGateway
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public AccountIdentityGateway(
        SignInManager<User> signInManager,
        UserManager<User> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<Result<AccountSignInResult>> SignInAsync(
        string email,
        string password,
        bool rememberMe,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _signInManager.PasswordSignInAsync(
            email,
            password,
            rememberMe,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return Result<AccountSignInResult>.Failure(AccountErrors.InvalidCredentials);
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            await _signInManager.SignOutAsync();
            return Result<AccountSignInResult>.Failure(AccountErrors.InvalidCredentials);
        }

        return Result<AccountSignInResult>.Success(
            new AccountSignInResult(user.Id, user.MustChangePassword));
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _signInManager.SignOutAsync();
    }

    public async Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            await _signInManager.SignOutAsync();
            return Result.Failure(AccountErrors.UserNotFound);
        }

        var changeResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!changeResult.Succeeded)
        {
            return Result.Failure(AccountErrors.PasswordChangeFailed(BuildIdentityErrorMessage(changeResult)));
        }

        user.MarkPasswordAsChanged();
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Result.Failure(AccountErrors.PasswordChangeFailed(BuildIdentityErrorMessage(updateResult)));
        }

        await _signInManager.RefreshSignInAsync(user);
        return Result.Success();
    }

    public async Task<Result<bool>> MustChangePasswordAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null
            ? Result<bool>.Failure(AccountErrors.UserNotFound)
            : Result<bool>.Success(user.MustChangePassword);
    }

    private static string BuildIdentityErrorMessage(IdentityResult result)
    {
        return string.Join("; ", result.Errors.Select(error => error.Description));
    }
}
