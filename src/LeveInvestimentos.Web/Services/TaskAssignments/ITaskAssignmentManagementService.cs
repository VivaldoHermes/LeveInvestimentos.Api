using System;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Application.Tarefas.DTOs;
using LeveInvestimentos.Web.ViewModels.TaskAssignments;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Web.Services.TaskAssignments;

public interface ITaskAssignmentManagementService
{
    Task<Result<TaskAssignmentIndexViewModel>> BuildIndexAsync(
        Guid currentUserId,
        bool isManager,
        TaskAssignmentStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<Result<CreateTaskAssignmentViewModel>> BuildCreateAsync(
        Guid managerId,
        CreateTaskAssignmentViewModel? model = null,
        CancellationToken cancellationToken = default);

    Task<Result<TaskAssignmentDetailsDto>> CreateAsync(
        Guid managerId,
        CreateTaskAssignmentViewModel model,
        CancellationToken cancellationToken = default);

    Task<Result<TaskAssignmentDetailsDto>> StartAsync(
        Guid currentUserId,
        Guid taskAssignmentId,
        CancellationToken cancellationToken = default);

    Task<Result<TaskAssignmentDetailsDto>> CompleteAsync(
        Guid currentUserId,
        Guid taskAssignmentId,
        CancellationToken cancellationToken = default);

    Task<Result<TaskAssignmentDetailsDto>> CancelAsync(
        Guid currentUserId,
        Guid taskAssignmentId,
        CancellationToken cancellationToken = default);
}
