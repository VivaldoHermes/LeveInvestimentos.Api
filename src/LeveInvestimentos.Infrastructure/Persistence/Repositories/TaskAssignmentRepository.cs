using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Infrastructure.Persistence.Repositories;

public sealed class TaskAssignmentRepository : ITaskAssignmentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TaskAssignmentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TaskAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.TaskAssignments
            .FirstOrDefaultAsync(taskAssignment => taskAssignment.Id == id, cancellationToken);
    }

    public async Task AddAsync(TaskAssignment taskAssignment, CancellationToken cancellationToken = default)
    {
        await _dbContext.TaskAssignments.AddAsync(taskAssignment, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TaskAssignment>> ListByManagerIdAsync(
        Guid managerId,
        TaskAssignmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var tasks = await ApplyStatusFilter(_dbContext.TaskAssignments.AsNoTracking(), status)
            .Where(taskAssignment => taskAssignment.ManagerId == managerId)
            .ToArrayAsync(cancellationToken);

        return tasks.OrderBy(taskAssignment => taskAssignment.DueDate).ToArray();
    }

    public async Task<IReadOnlyCollection<TaskAssignment>> ListBySubordinateIdAsync(
        Guid subordinateId,
        TaskAssignmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var tasks = await ApplyStatusFilter(_dbContext.TaskAssignments.AsNoTracking(), status)
            .Where(taskAssignment => taskAssignment.SubordinateId == subordinateId)
            .ToArrayAsync(cancellationToken);

        return tasks.OrderBy(taskAssignment => taskAssignment.DueDate).ToArray();
    }

    private static IQueryable<TaskAssignment> ApplyStatusFilter(
        IQueryable<TaskAssignment> query,
        TaskAssignmentStatus? status)
    {
        return status is null
            ? query
            : query.Where(taskAssignment => taskAssignment.Status == status);
    }
}
