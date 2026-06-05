using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Application.Tarefas.Commands;
using LeveInvestimentos.Core.Application.Tarefas.DTOs;
using LeveInvestimentos.Core.Application.Tarefas.Services;
using LeveInvestimentos.Core.Application.Users.DTOs;
using LeveInvestimentos.Core.Application.Users.Services;
using LeveInvestimentos.Web.ViewModels.TaskAssignments;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Web.Services.TaskAssignments;

public sealed class TaskAssignmentManagementService : ITaskAssignmentManagementService
{
    private readonly ITaskAssignmentService _taskAssignmentService;
    private readonly IUserService _userService;

    public TaskAssignmentManagementService(
        ITaskAssignmentService taskAssignmentService,
        IUserService userService)
    {
        _taskAssignmentService = taskAssignmentService;
        _userService = userService;
    }

    public async Task<Result<TaskAssignmentIndexViewModel>> BuildIndexAsync(
        Guid currentUserId,
        bool isManager,
        TaskAssignmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var tasksResult = isManager
            ? await _taskAssignmentService.ListForManagerAsync(currentUserId, status, cancellationToken)
            : await _taskAssignmentService.ListForSubordinateAsync(currentUserId, status, cancellationToken);

        if (tasksResult.IsFailure)
        {
            return Result<TaskAssignmentIndexViewModel>.Failure(tasksResult.Error);
        }

        var currentUserResult = await _userService.GetByIdAsync(currentUserId, cancellationToken);
        if (currentUserResult.IsFailure)
        {
            return Result<TaskAssignmentIndexViewModel>.Failure(currentUserResult.Error);
        }

        var subordinatesResult = isManager
            ? await _userService.ListSubordinatesAsync(currentUserId, cancellationToken)
            : null;

        if (subordinatesResult?.IsFailure == true)
        {
            return Result<TaskAssignmentIndexViewModel>.Failure(subordinatesResult.Error);
        }

        var model = new TaskAssignmentIndexViewModel
        {
            Filter = new TaskAssignmentFilterViewModel
            {
                Status = status,
                StatusOptions = BuildStatusOptions(status)
            },
            Tasks = tasksResult.Value
                .Select(task => MapListItem(
                    task,
                    currentUserResult.Value,
                    subordinatesResult?.Value ?? Array.Empty<UserListItemDto>(),
                    isManager))
                .ToArray(),
            CanCreate = isManager
        };

        return Result<TaskAssignmentIndexViewModel>.Success(model);
    }

    public async Task<Result<CreateTaskAssignmentViewModel>> BuildCreateAsync(
        Guid managerId,
        CreateTaskAssignmentViewModel? model = null,
        CancellationToken cancellationToken = default)
    {
        var subordinatesResult = await _userService.ListSubordinatesAsync(managerId, cancellationToken);
        if (subordinatesResult.IsFailure)
        {
            return Result<CreateTaskAssignmentViewModel>.Failure(subordinatesResult.Error);
        }

        var viewModel = model ?? new CreateTaskAssignmentViewModel();
        viewModel.Subordinates = subordinatesResult.Value
            .Select(user => new SelectListItem(user.FullName, user.Id.ToString(), user.Id == viewModel.SubordinateId))
            .ToArray();

        return Result<CreateTaskAssignmentViewModel>.Success(viewModel);
    }

    public Task<Result<TaskAssignmentDetailsDto>> CreateAsync(
        Guid managerId,
        CreateTaskAssignmentViewModel model,
        CancellationToken cancellationToken = default)
    {
        return _taskAssignmentService.CreateAsync(
            new CreateTaskAssignmentCommand(
                managerId,
                model.SubordinateId!.Value,
                model.Description,
                ToDueDate(model.DueDate!.Value)),
            cancellationToken);
    }

    public Task<Result<TaskAssignmentDetailsDto>> StartAsync(
        Guid currentUserId,
        Guid taskAssignmentId,
        CancellationToken cancellationToken = default)
    {
        return _taskAssignmentService.StartAsync(
            new StartTaskAssignmentCommand(taskAssignmentId, currentUserId),
            cancellationToken);
    }

    public Task<Result<TaskAssignmentDetailsDto>> CompleteAsync(
        Guid currentUserId,
        Guid taskAssignmentId,
        CancellationToken cancellationToken = default)
    {
        return _taskAssignmentService.CompleteAsync(
            new CompleteTaskAssignmentCommand(taskAssignmentId, currentUserId),
            cancellationToken);
    }

    public Task<Result<TaskAssignmentDetailsDto>> CancelAsync(
        Guid currentUserId,
        Guid taskAssignmentId,
        CancellationToken cancellationToken = default)
    {
        return _taskAssignmentService.CancelAsync(
            new CancelTaskAssignmentCommand(taskAssignmentId, currentUserId),
            cancellationToken);
    }

    private static TaskAssignmentListItemViewModel MapListItem(
        TaskAssignmentListItemDto task,
        UserDetailsDto currentUser,
        IReadOnlyCollection<UserListItemDto> subordinates,
        bool isManager)
    {
        var subordinateName = isManager
            ? subordinates.FirstOrDefault(user => user.Id == task.SubordinateId)?.FullName ?? "Subordinado"
            : currentUser.FullName;

        var managerName = isManager
            ? currentUser.FullName
            : currentUser.ManagerName ?? "Gestor";

        return new TaskAssignmentListItemViewModel
        {
            Id = task.Id,
            Description = task.Description,
            DueDate = task.DueDate,
            Status = task.Status,
            ManagerName = managerName,
            SubordinateName = subordinateName,
            IsOverdue = task.IsOverdue,
            CanStart = !isManager && task.Status == TaskAssignmentStatus.Pending,
            CanComplete = !isManager && task.Status is TaskAssignmentStatus.Pending or TaskAssignmentStatus.Started,
            CanCancel = isManager && task.Status is (TaskAssignmentStatus.Pending or TaskAssignmentStatus.Started)
        };
    }

    private static DateTimeOffset ToDueDate(DateTime dueDate)
    {
        var localDueDate = DateTime.SpecifyKind(dueDate, DateTimeKind.Local);
        return new DateTimeOffset(localDueDate).ToUniversalTime();
    }

    private static IReadOnlyCollection<SelectListItem> BuildStatusOptions(TaskAssignmentStatus? selectedStatus)
    {
        return new[]
        {
            BuildStatusOption(null, "Todos", selectedStatus),
            BuildStatusOption(TaskAssignmentStatus.Pending, "Pendente", selectedStatus),
            BuildStatusOption(TaskAssignmentStatus.Started, "Em andamento", selectedStatus),
            BuildStatusOption(TaskAssignmentStatus.Completed, "Concluida", selectedStatus),
            BuildStatusOption(TaskAssignmentStatus.Canceled, "Cancelada", selectedStatus)
        };
    }

    private static SelectListItem BuildStatusOption(
        TaskAssignmentStatus? status,
        string text,
        TaskAssignmentStatus? selectedStatus)
    {
        return new SelectListItem(
            text,
            status?.ToString() ?? string.Empty,
            status == selectedStatus);
    }
}
