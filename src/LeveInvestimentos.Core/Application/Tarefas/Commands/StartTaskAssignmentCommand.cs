using System;

namespace LeveInvestimentos.Core.Application.Tarefas.Commands;

public sealed record StartTaskAssignmentCommand(Guid TaskAssignmentId, Guid CurrentUserId);
