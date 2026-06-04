using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Infrastructure.Outbox;
using LeveInvestimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LeveInvestimentos.Infrastructure.BackgroundServices;

public sealed class OutboxDispatcherService : BackgroundService
{
    private const int BatchSize = 20;
    private static readonly TimeSpan CycleDelay = TimeSpan.FromSeconds(10);
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcherService> _logger;

    public OutboxDispatcherService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxDispatcherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected failure while processing outbox messages.");
            }

            await Task.Delay(CycleDelay, stoppingToken);
        }
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var now = DateTimeOffset.UtcNow;

        var messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedAt == null)
            .Where(message => message.NextAttemptAt == null || message.NextAttemptAt <= now)
            .OrderBy(message => message.OccurredAt)
            .Take(BatchSize)
            .ToArrayAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                if (message.Type != EmailOutbox.MessageType)
                {
                    throw new InvalidOperationException($"Unsupported outbox message type '{message.Type}'.");
                }

                var payload = JsonSerializer.Deserialize<EmailOutboxMessagePayload>(
                    message.PayloadJson,
                    SerializerOptions);

                if (payload is null)
                {
                    throw new InvalidOperationException("Outbox message payload is invalid.");
                }

                await emailSender.SendAsync(payload.To, payload.Subject, payload.Body, cancellationToken);
                message.MarkProcessed(DateTimeOffset.UtcNow);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                message.RegisterFailure(ex.Message, DateTimeOffset.UtcNow);
                _logger.LogWarning(
                    ex,
                    "Failed to process outbox message {OutboxMessageId}. Attempt {Attempt}.",
                    message.Id,
                    message.Attempts);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
