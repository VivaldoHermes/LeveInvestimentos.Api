using System;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Application.Account.DTOs;
using LeveInvestimentos.Core.Application.Common;

namespace LeveInvestimentos.Core.Abstractions;

public interface IAccountIdentityGateway
{
    Task<Result<AccountSignInResult>> SignInAsync(
        string email,
        string password,
        bool rememberMe,
        CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> MustChangePasswordAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
