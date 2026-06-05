using System;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Core.Application.Account.Commands;
using LeveInvestimentos.Core.Application.Account.DTOs;
using LeveInvestimentos.Core.Application.Common;

namespace LeveInvestimentos.Core.Application.Account.Services;

public sealed class AccountService : IAccountService
{
    private readonly IAccountIdentityGateway _identityGateway;

    public AccountService(IAccountIdentityGateway identityGateway)
    {
        _identityGateway = identityGateway;
    }

    public Task<Result<AccountSignInResult>> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
        {
            return Task.FromResult(Result<AccountSignInResult>.Failure(AccountErrors.InvalidCredentials));
        }

        return _identityGateway.SignInAsync(
            command.Email.Trim(),
            command.Password,
            command.RememberMe,
            cancellationToken);
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        return _identityGateway.SignOutAsync(cancellationToken);
    }

    public Task<Result> ChangePasswordAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.UserId == Guid.Empty)
        {
            return Task.FromResult(Result.Failure(AccountErrors.UserIdRequired));
        }

        if (string.IsNullOrWhiteSpace(command.CurrentPassword)
            || string.IsNullOrWhiteSpace(command.NewPassword))
        {
            return Task.FromResult(Result.Failure(AccountErrors.PasswordDataRequired));
        }

        return _identityGateway.ChangePasswordAsync(
            command.UserId,
            command.CurrentPassword,
            command.NewPassword,
            cancellationToken);
    }

    public Task<Result<bool>> MustChangePasswordAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return Task.FromResult(Result<bool>.Failure(AccountErrors.UserIdRequired));
        }

        return _identityGateway.MustChangePasswordAsync(userId, cancellationToken);
    }
}
