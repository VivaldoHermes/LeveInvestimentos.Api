using System;
using LeveInvestimentos.Core.Application.Common;

namespace LeveInvestimentos.Core.Application.Tarefas.Commands;

public static class TaskAssignmentCommandValidators
{
    public static Result Validate(CreateTaskAssignmentCommand command, DateTimeOffset now)
    {
        if (command.ManagerId == Guid.Empty)
        {
            return Result.Failure(new Error("TaskAssignments.ManagerRequired", "O gestor é obrigatório."));
        }

        if (command.SubordinateId == Guid.Empty)
        {
            return Result.Failure(new Error("TaskAssignments.SubordinateRequired", "O subordinado é obrigatório."));
        }

        if (command.ManagerId == command.SubordinateId)
        {
            return Result.Failure(new Error("TaskAssignments.InvalidParticipants", "O gestor e o subordinado devem ser usuários diferentes."));
        }

        if (string.IsNullOrWhiteSpace(command.Description))
        {
            return Result.Failure(new Error("TaskAssignments.DescriptionRequired", "A descrição é obrigatória."));
        }

        if (command.Description.Trim().Length > 2_000)
        {
            return Result.Failure(new Error("TaskAssignments.DescriptionTooLong", "A descrição não pode exceder 2000 caracteres."));
        }

        if (command.DueDate.ToUniversalTime() <= now.ToUniversalTime())
        {
            return Result.Failure(new Error("TaskAssignments.InvalidDueDate", "A data limite deve ser futura."));
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
            return Result.Failure(new Error("TaskAssignments.IdRequired", "A tarefa é obrigatória."));
        }

        if (currentUserId == Guid.Empty)
        {
            return Result.Failure(new Error("TaskAssignments.CurrentUserRequired", "O usuário autenticado é obrigatório."));
        }

        return Result.Success();
    }
}
