using System;

namespace LeveInvestimentos.Core.Domain.Events;

public sealed record TaskAssignmentCreatedEvent(
    Guid TaskAssignmentId,
    Guid ManagerId,
    Guid SubordinateId,
    string Description,
    DateTimeOffset DueDate,
    DateTimeOffset OccurredAt) : IDomainEvent;
