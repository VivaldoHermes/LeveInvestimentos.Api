using System;

namespace LeveInvestimentos.Core.Application.Tarefas.Commands;

public sealed record CancelTaskAssignmentCommand(Guid TaskAssignmentId, Guid CurrentUserId);
