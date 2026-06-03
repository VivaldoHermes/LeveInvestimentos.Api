using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Application.Tarefas.Commands;
using LeveInvestimentos.Core.Application.Tarefas.DTOs;
using LeveInvestimentos.Core.Domain.Entities;
using LeveInvestimentos.Core.Domain.Enums;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Core.Application.Tarefas.Services;

public sealed class TaskAssignmentService : ITaskAssignmentService
{
    private readonly ITaskAssignmentRepository _taskAssignmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TaskAssignmentService(
        ITaskAssignmentRepository taskAssignmentRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _taskAssignmentRepository = taskAssignmentRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TaskAssignmentDetailsDto>> CreateAsync(
        CreateTaskAssignmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var validation = TaskAssignmentCommandValidators.Validate(command, now);
        if (validation.IsFailure)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(validation.Error);
        }

        var manager = await _userRepository.GetByIdAsync(command.ManagerId, cancellationToken);
        if (manager is null || manager.Role != UserRole.Manager)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(new Error("TaskAssignments.InvalidManager", "Manager was not found."));
        }

        var subordinate = await _userRepository.GetByIdAsync(command.SubordinateId, cancellationToken);
        if (subordinate is null || subordinate.Role != UserRole.Subordinate)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(new Error("TaskAssignments.InvalidSubordinate", "Subordinate was not found."));
        }

        if (subordinate.ManagerId != manager.Id)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(new Error("TaskAssignments.SubordinateNotManaged", "Subordinate does not belong to this manager."));
        }

        var taskAssignment = TaskAssignment.Create(
            command.ManagerId,
            command.SubordinateId,
            command.Description,
            command.DueDate,
            now);

        await _taskAssignmentRepository.AddAsync(taskAssignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TaskAssignmentDetailsDto>.Success(MapDetails(taskAssignment, manager.FullName, subordinate.FullName, now));
    }

    public async Task<Result<TaskAssignmentDetailsDto>> StartAsync(
        StartTaskAssignmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = TaskAssignmentCommandValidators.Validate(command);
        if (validation.IsFailure)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(validation.Error);
        }

        var taskAssignment = await _taskAssignmentRepository.GetByIdAsync(command.TaskAssignmentId, cancellationToken);
        if (taskAssignment is null)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(new Error("TaskAssignments.NotFound", "Task assignment was not found."));
        }

        if (taskAssignment.SubordinateId != command.CurrentUserId)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(new Error("TaskAssignments.Forbidden", "Only the responsible subordinate can start this task."));
        }

        try
        {
            taskAssignment.Start();
        }
        catch (InvalidOperationException ex)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(new Error("TaskAssignments.InvalidTransition", ex.Message));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TaskAssignmentDetailsDto>.Success(MapDetails(taskAssignment, null, null, DateTimeOffset.UtcNow));
    }

    public async Task<Result<TaskAssignmentDetailsDto>> CompleteAsync(
        CompleteTaskAssignmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = TaskAssignmentCommandValidators.Validate(command);
        if (validation.IsFailure)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(validation.Error);
        }

        var taskAssignment = await _taskAssignmentRepository.GetByIdAsync(command.TaskAssignmentId, cancellationToken);
        if (taskAssignment is null)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(new Error("TaskAssignments.NotFound", "Task assignment was not found."));
        }

        if (taskAssignment.SubordinateId != command.CurrentUserId)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(new Error("TaskAssignments.Forbidden", "Only the responsible subordinate can complete this task."));
        }

        try
        {
            taskAssignment.Complete(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(new Error("TaskAssignments.InvalidTransition", ex.Message));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TaskAssignmentDetailsDto>.Success(MapDetails(taskAssignment, null, null, DateTimeOffset.UtcNow));
    }

    public async Task<Result<TaskAssignmentDetailsDto>> CancelAsync(
        CancelTaskAssignmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = TaskAssignmentCommandValidators.Validate(command);
        if (validation.IsFailure)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(validation.Error);
        }

        var taskAssignment = await _taskAssignmentRepository.GetByIdAsync(command.TaskAssignmentId, cancellationToken);
        if (taskAssignment is null)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(new Error("TaskAssignments.NotFound", "Task assignment was not found."));
        }

        if (taskAssignment.ManagerId != command.CurrentUserId)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(new Error("TaskAssignments.Forbidden", "Only the manager that created this task can cancel it."));
        }

        try
        {
            taskAssignment.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            return Result<TaskAssignmentDetailsDto>.Failure(new Error("TaskAssignments.InvalidTransition", ex.Message));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TaskAssignmentDetailsDto>.Success(MapDetails(taskAssignment, null, null, DateTimeOffset.UtcNow));
    }

    public async Task<Result<IReadOnlyCollection<TaskAssignmentListItemDto>>> ListForManagerAsync(
        Guid managerId,
        TaskAssignmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        if (managerId == Guid.Empty)
        {
            return Result<IReadOnlyCollection<TaskAssignmentListItemDto>>.Failure(new Error("TaskAssignments.ManagerRequired", "Manager id is required."));
        }

        var taskAssignments = await _taskAssignmentRepository.ListByManagerIdAsync(managerId, status, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        return Result<IReadOnlyCollection<TaskAssignmentListItemDto>>.Success(taskAssignments.Select(taskAssignment => MapListItem(taskAssignment, now)).ToArray());
    }

    public async Task<Result<IReadOnlyCollection<TaskAssignmentListItemDto>>> ListForSubordinateAsync(
        Guid subordinateId,
        TaskAssignmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        if (subordinateId == Guid.Empty)
        {
            return Result<IReadOnlyCollection<TaskAssignmentListItemDto>>.Failure(new Error("TaskAssignments.SubordinateRequired", "Subordinate id is required."));
        }

        var taskAssignments = await _taskAssignmentRepository.ListBySubordinateIdAsync(subordinateId, status, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        return Result<IReadOnlyCollection<TaskAssignmentListItemDto>>.Success(taskAssignments.Select(taskAssignment => MapListItem(taskAssignment, now)).ToArray());
    }

    private static TaskAssignmentListItemDto MapListItem(TaskAssignment taskAssignment, DateTimeOffset now)
    {
        return new TaskAssignmentListItemDto(
            taskAssignment.Id,
            taskAssignment.Description,
            taskAssignment.DueDate,
            taskAssignment.Status,
            taskAssignment.ManagerId,
            null,
            taskAssignment.SubordinateId,
            null,
            taskAssignment.CreatedAt,
            taskAssignment.CompletedAt,
            taskAssignment.IsOverdue(now));
    }

    private static TaskAssignmentDetailsDto MapDetails(
        TaskAssignment taskAssignment,
        string? managerName,
        string? subordinateName,
        DateTimeOffset now)
    {
        return new TaskAssignmentDetailsDto(
            taskAssignment.Id,
            taskAssignment.Description,
            taskAssignment.DueDate,
            taskAssignment.Status,
            taskAssignment.ManagerId,
            managerName,
            taskAssignment.SubordinateId,
            subordinateName,
            taskAssignment.CreatedAt,
            taskAssignment.CompletedAt,
            taskAssignment.IsOverdue(now));
    }
}
