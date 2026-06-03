using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Domain.Entities;

namespace LeveInvestimentos.Core.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<User>> ListByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<User>> ListManagedUsersAsync(Guid managerId, CancellationToken cancellationToken = default);

    Task AddAsync(User user, string password, CancellationToken cancellationToken = default);
}
