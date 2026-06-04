using System;

namespace LeveInvestimentos.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
        Type = string.Empty;
        PayloadJson = string.Empty;
    }

    public OutboxMessage(string type, string payloadJson, DateTimeOffset occurredAt)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Outbox message type is required.", nameof(type));
        }

        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            throw new ArgumentException("Outbox message payload is required.", nameof(payloadJson));
        }

        Id = Guid.NewGuid();
        Type = type.Trim();
        PayloadJson = payloadJson.Trim();
        OccurredAt = occurredAt.Offset == TimeSpan.Zero ? occurredAt : occurredAt.ToUniversalTime();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; }

    public string PayloadJson { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public int Attempts { get; private set; }

    public string? LastError { get; private set; }

    public DateTimeOffset? NextAttemptAt { get; private set; }

    public void MarkProcessed(DateTimeOffset processedAt)
    {
        ProcessedAt = processedAt.Offset == TimeSpan.Zero ? processedAt : processedAt.ToUniversalTime();
        LastError = null;
        NextAttemptAt = null;
    }

    public void RegisterFailure(string error, DateTimeOffset failedAt)
    {
        Attempts++;
        LastError = string.IsNullOrWhiteSpace(error) ? null : error.Trim();

        var normalizedFailedAt = failedAt.Offset == TimeSpan.Zero ? failedAt : failedAt.ToUniversalTime();
        var delaySeconds = Math.Min(Math.Pow(2, Attempts) * 30, 300);
        NextAttemptAt = normalizedFailedAt.AddSeconds(delaySeconds);
    }
}
