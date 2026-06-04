namespace LeveInvestimentos.Infrastructure.Outbox;

public sealed record EmailOutboxMessagePayload(
    string To,
    string Subject,
    string Body);
