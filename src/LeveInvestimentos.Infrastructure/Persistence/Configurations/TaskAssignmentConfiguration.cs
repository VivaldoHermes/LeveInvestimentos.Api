using LeveInvestimentos.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeveInvestimentos.Infrastructure.Persistence.Configurations;

public sealed class TaskAssignmentConfiguration : IEntityTypeConfiguration<TaskAssignment>
{
    public void Configure(EntityTypeBuilder<TaskAssignment> builder)
    {
        builder.ToTable("TaskAssignments");

        builder.HasKey(taskAssignment => taskAssignment.Id);

        builder.Property(taskAssignment => taskAssignment.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(taskAssignment => taskAssignment.DueDate)
            .IsRequired();

        builder.Property(taskAssignment => taskAssignment.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(taskAssignment => taskAssignment.ManagerId)
            .IsRequired();

        builder.Property(taskAssignment => taskAssignment.SubordinateId)
            .IsRequired();

        builder.Property(taskAssignment => taskAssignment.CreatedAt)
            .IsRequired();

        builder.Property(taskAssignment => taskAssignment.CompletedAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(taskAssignment => taskAssignment.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(taskAssignment => taskAssignment.SubordinateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(taskAssignment => taskAssignment.ManagerId);
        builder.HasIndex(taskAssignment => taskAssignment.SubordinateId);
        builder.HasIndex(taskAssignment => taskAssignment.Status);
    }
}
