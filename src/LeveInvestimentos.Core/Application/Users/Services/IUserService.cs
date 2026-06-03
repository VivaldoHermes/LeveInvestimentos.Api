using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Application.Users.Commands;
using LeveInvestimentos.Core.Application.Users.DTOs;

namespace LeveInvestimentos.Core.Application.Users.Services;

public interface IUserService
{
    Task<Result<UserDetailsDto>> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken = default);

    Task<Result<UserDetailsDto>> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<UserListItemDto>>> ListSubordinatesAsync(Guid managerId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<UserListItemDto>>> ListManagedUsersAsync(Guid managerId, CancellationToken cancellationToken = default);
}
