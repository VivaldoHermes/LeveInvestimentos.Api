using System;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Core.Application.Tarefas.DTOs;

public sealed record TaskAssignmentListItemDto(
    Guid Id,
    string Description,
    DateTimeOffset DueDate,
    TaskAssignmentStatus Status,
    Guid ManagerId,
    string? ManagerName,
    Guid SubordinateId,
    string? SubordinateName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    bool IsOverdue);
