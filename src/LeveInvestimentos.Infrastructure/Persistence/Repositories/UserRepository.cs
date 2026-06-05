using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Core.Domain.Entities;
using LeveInvestimentos.Core.Domain.Enums;
using LeveInvestimentos.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LeveInvestimentos.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<User> _userManager;

    public UserRepository(ApplicationDbContext dbContext, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .Include(user => user.Manager)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Email == email, cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> ListByManagerIdAsync(
        Guid managerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.ManagerId == managerId)
            .OrderBy(user => user.FullName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> ListManagedUsersAsync(
        Guid managerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Include(user => user.Manager)
            .Where(user => user.Id == managerId || user.ManagerId == managerId)
            .OrderBy(user => user.FullName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        var roleName = user.Role == UserRole.Manager
            ? IdentityRoleNames.Manager
            : IdentityRoleNames.Subordinate;

        var roleResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(error => error.Description)));
        }
    }
}
