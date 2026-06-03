using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Application.Tarefas.Commands;
using LeveInvestimentos.Core.Application.Tarefas.DTOs;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Core.Application.Tarefas.Services;

public interface ITaskAssignmentService
{
    Task<Result<TaskAssignmentDetailsDto>> CreateAsync(CreateTaskAssignmentCommand command, CancellationToken cancellationToken = default);

    Task<Result<TaskAssignmentDetailsDto>> StartAsync(StartTaskAssignmentCommand command, CancellationToken cancellationToken = default);

    Task<Result<TaskAssignmentDetailsDto>> CompleteAsync(CompleteTaskAssignmentCommand command, CancellationToken cancellationToken = default);

    Task<Result<TaskAssignmentDetailsDto>> CancelAsync(CancelTaskAssignmentCommand command, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<TaskAssignmentListItemDto>>> ListForManagerAsync(
        Guid managerId,
        TaskAssignmentStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<TaskAssignmentListItemDto>>> ListForSubordinateAsync(
        Guid subordinateId,
        TaskAssignmentStatus? status = null,
        CancellationToken cancellationToken = default);
}
