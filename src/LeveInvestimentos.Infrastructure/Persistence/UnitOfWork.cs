using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeveInvestimentos.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public UnitOfWork(ApplicationDbContext dbContext, IDomainEventDispatcher domainEventDispatcher)
    {
        _dbContext = dbContext;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = _dbContext.ChangeTracker
            .Entries<DomainEntity>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToArray();

        var domainEvents = domainEntities
            .SelectMany(entity => entity.DomainEvents)
            .ToArray();

        if (domainEvents.Length > 0)
        {
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

            foreach (var entity in domainEntities)
            {
                entity.ClearDomainEvents();
            }
        }

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
