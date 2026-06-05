using System;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Application.Account.Commands;
using LeveInvestimentos.Core.Application.Account.DTOs;
using LeveInvestimentos.Core.Application.Common;

namespace LeveInvestimentos.Core.Application.Account.Services;

public interface IAccountService
{
    Task<Result<AccountSignInResult>> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> MustChangePasswordAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
