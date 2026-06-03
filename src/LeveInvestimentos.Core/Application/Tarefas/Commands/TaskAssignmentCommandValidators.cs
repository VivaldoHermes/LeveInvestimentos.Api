using System;
using LeveInvestimentos.Core.Application.Common;

namespace LeveInvestimentos.Core.Application.Tarefas.Commands;

public static class TaskAssignmentCommandValidators
{
    public static Result Validate(CreateTaskAssignmentCommand command, DateTimeOffset now)
    {
        if (command.ManagerId == Guid.Empty)
        {
            return Result.Failure(new Error("TaskAssignments.ManagerRequired", "Manager id is required."));
        }

        if (command.SubordinateId == Guid.Empty)
        {
            return Result.Failure(new Error("TaskAssignments.SubordinateRequired", "Subordinate id is required."));
        }

        if (command.ManagerId == command.SubordinateId)
        {
            return Result.Failure(new Error("TaskAssignments.InvalidParticipants", "Manager and subordinate must be different users."));
        }

        if (string.IsNullOrWhiteSpace(command.Description))
        {
            return Result.Failure(new Error("TaskAssignments.DescriptionRequired", "Description is required."));
        }

        if (command.Description.Trim().Length > 2_000)
        {
            return Result.Failure(new Error("TaskAssignments.DescriptionTooLong", "Description cannot exceed 2000 characters."));
        }

        if (command.DueDate.ToUniversalTime() <= now.ToUniversalTime())
        {
            return Result.Failure(new Error("TaskAssignments.InvalidDueDate", "Due date must be in the future."));
        }

        return Result.Success();
    }

    public static Result Validate(StartTaskAssignmentCommand command)
    {
        return ValidateTaskAndUser(command.TaskAssignmentId, command.CurrentUserId);
    }

    public static Result Validate(CompleteTaskAssignmentCommand command)
    {
        return ValidateTaskAndUser(command.TaskAssignmentId, command.CurrentUserId);
    }

    public static Result Validate(CancelTaskAssignmentCommand command)
    {
        return ValidateTaskAndUser(command.TaskAssignmentId, command.CurrentUserId);
    }

    private static Result ValidateTaskAndUser(Guid taskAssignmentId, Guid currentUserId)
    {
        if (taskAssignmentId == Guid.Empty)
        {
            return Result.Failure(new Error("TaskAssignments.IdRequired", "Task assignment id is required."));
        }

        if (currentUserId == Guid.Empty)
        {
            return Result.Failure(new Error("TaskAssignments.CurrentUserRequired", "Current user id is required."));
        }

        return Result.Success();
    }
}
