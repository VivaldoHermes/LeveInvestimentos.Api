using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Application.Users.DTOs;
using LeveInvestimentos.Web.ViewModels.Users;

namespace LeveInvestimentos.Web.Services.Users;

public interface IUserManagementService
{
    Task<Result<IReadOnlyCollection<UserListItemViewModel>>> ListManagedUsersAsync(
        Guid managerId,
        CancellationToken cancellationToken = default);

    Task<Result<UserDetailsDto>> CreateAsync(
        Guid managerId,
        CreateUserViewModel model,
        CancellationToken cancellationToken = default);
}
