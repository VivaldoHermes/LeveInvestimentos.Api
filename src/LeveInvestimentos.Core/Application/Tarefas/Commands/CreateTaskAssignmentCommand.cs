using System;

namespace LeveInvestimentos.Core.Application.Tarefas.Commands;

public sealed record CreateTaskAssignmentCommand(
    Guid ManagerId,
    Guid SubordinateId,
    string Description,
    DateTimeOffset DueDate);
