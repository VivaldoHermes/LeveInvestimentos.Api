using System;

namespace LeveInvestimentos.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
        Type = string.Empty;
        Content = string.Empty;
    }

    public OutboxMessage(string type, string content, DateTimeOffset occurredAt)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Outbox message type is required.", nameof(type));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Outbox message content is required.", nameof(content));
        }

        Id = Guid.NewGuid();
        Type = type.Trim();
        Content = content.Trim();
        OccurredAt = occurredAt.Offset == TimeSpan.Zero ? occurredAt : occurredAt.ToUniversalTime();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; }

    public string Content { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public int Attempts { get; private set; }

    public string? LastError { get; private set; }

    public void MarkProcessed(DateTimeOffset processedAt)
    {
        ProcessedAt = processedAt.Offset == TimeSpan.Zero ? processedAt : processedAt.ToUniversalTime();
    }

    public void RegisterFailure(string error)
    {
        Attempts++;
        LastError = string.IsNullOrWhiteSpace(error) ? null : error.Trim();
    }
}
