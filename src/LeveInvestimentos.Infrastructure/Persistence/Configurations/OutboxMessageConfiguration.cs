using LeveInvestimentos.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeveInvestimentos.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Type)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(message => message.PayloadJson)
            .IsRequired();

        builder.Property(message => message.OccurredAt)
            .IsRequired();

        builder.Property(message => message.CreatedAt)
            .IsRequired();

        builder.Property(message => message.LastError)
            .HasMaxLength(2000);

        builder.HasIndex(message => message.ProcessedAt);
        builder.HasIndex(message => message.NextAttemptAt);
    }
}
