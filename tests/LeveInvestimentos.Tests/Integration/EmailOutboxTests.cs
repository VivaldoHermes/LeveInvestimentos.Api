using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LeveInvestimentos.Infrastructure.Outbox;
using LeveInvestimentos.Tests.TestSupport;
using Xunit;

namespace LeveInvestimentos.Tests.Integration;

public sealed class EmailOutboxTests
{
    [Fact]
    public async Task EnqueueAsync_WithValidData_AddsTrimmedEmailMessage()
    {
        using var factory = new SqliteDbContextFactory();
        await using var context = factory.CreateContext();
        var outbox = new EmailOutbox(context);

        await outbox.EnqueueAsync(" user@example.com ", " Assunto ", " Corpo ");
        await context.SaveChangesAsync();

        var message = context.OutboxMessages.Single();
        message.Type.Should().Be(EmailOutbox.MessageType);
        message.ProcessedAt.Should().BeNull();

        using var payload = JsonDocument.Parse(message.PayloadJson);
        payload.RootElement.GetProperty("to").GetString().Should().Be("user@example.com");
        payload.RootElement.GetProperty("subject").GetString().Should().Be("Assunto");
        payload.RootElement.GetProperty("body").GetString().Should().Be("Corpo");
    }

    [Theory]
    [InlineData("", "Assunto", "Corpo", "to")]
    [InlineData("user@example.com", "", "Corpo", "subject")]
    [InlineData("user@example.com", "Assunto", "", "body")]
    public async Task EnqueueAsync_WithBlankRequiredData_Throws(string to, string subject, string body, string parameterName)
    {
        using var factory = new SqliteDbContextFactory();
        await using var context = factory.CreateContext();
        var outbox = new EmailOutbox(context);

        Func<Task> act = () => outbox.EnqueueAsync(to, subject, body);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(parameterName);
    }
}
