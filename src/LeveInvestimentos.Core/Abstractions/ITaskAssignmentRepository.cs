using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Domain.Entities;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Core.Abstractions;

public interface ITaskAssignmentRepository
{
    Task<TaskAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(TaskAssignment taskAssignment, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TaskAssignment>> ListByManagerIdAsync(
        Guid managerId,
        TaskAssignmentStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TaskAssignment>> ListBySubordinateIdAsync(
        Guid subordinateId,
        TaskAssignmentStatus? status = null,
        CancellationToken cancellationToken = default);
}
