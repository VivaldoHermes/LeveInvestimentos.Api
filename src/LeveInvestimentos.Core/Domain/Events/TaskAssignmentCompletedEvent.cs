using System;

namespace LeveInvestimentos.Core.Domain.Events;

public sealed record TaskAssignmentCompletedEvent(
    Guid TaskAssignmentId,
    Guid ManagerId,
    Guid SubordinateId,
    string Description,
    DateTimeOffset CompletedAt,
    DateTimeOffset OccurredAt) : IDomainEvent;
