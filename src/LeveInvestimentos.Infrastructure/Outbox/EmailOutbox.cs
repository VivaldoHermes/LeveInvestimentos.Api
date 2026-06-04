using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Infrastructure.Persistence;

namespace LeveInvestimentos.Infrastructure.Outbox;

public sealed class EmailOutbox : IEmailOutbox
{
    public const string MessageType = "Email";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly ApplicationDbContext _dbContext;

    public EmailOutbox(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnqueueAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(to))
        {
            throw new ArgumentException("Recipient e-mail is required.", nameof(to));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("E-mail subject is required.", nameof(subject));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("E-mail body is required.", nameof(body));
        }

        var payload = new EmailOutboxMessagePayload(to.Trim(), subject.Trim(), body.Trim());
        var payloadJson = JsonSerializer.Serialize(payload, SerializerOptions);

        await _dbContext.OutboxMessages.AddAsync(
            new OutboxMessage(MessageType, payloadJson, DateTimeOffset.UtcNow),
            cancellationToken);
    }
}
