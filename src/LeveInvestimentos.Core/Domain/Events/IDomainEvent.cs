using System;

namespace LeveInvestimentos.Core.Domain.Events;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
