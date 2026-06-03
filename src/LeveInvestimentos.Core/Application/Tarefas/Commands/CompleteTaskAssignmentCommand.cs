using System;

namespace LeveInvestimentos.Core.Application.Tarefas.Commands;

public sealed record CompleteTaskAssignmentCommand(Guid TaskAssignmentId, Guid CurrentUserId);
