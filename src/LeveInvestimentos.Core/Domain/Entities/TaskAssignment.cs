using System;
using LeveInvestimentos.Core.Domain.Events;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Core.Domain.Entities;

public sealed class TaskAssignment : DomainEntity
{
    private TaskAssignment()
    {
        Description = string.Empty;
    }

    private TaskAssignment(
        Guid id,
        string description,
        DateTimeOffset dueDate,
        Guid managerId,
        Guid subordinateId,
        DateTimeOffset createdAt)
    {
        Id = id;
        Description = description;
        DueDate = dueDate;
        ManagerId = managerId;
        SubordinateId = subordinateId;
        CreatedAt = createdAt;
        Status = TaskAssignmentStatus.Pending;
    }

    public Guid Id { get; private set; }

    public string Description { get; private set; }

    public DateTimeOffset DueDate { get; private set; }

    public TaskAssignmentStatus Status { get; private set; }

    public Guid ManagerId { get; private set; }

    public Guid SubordinateId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public static TaskAssignment Create(
        Guid managerId,
        Guid subordinateId,
        string description,
        DateTimeOffset dueDate,
        DateTimeOffset? createdAt = null)
    {
        if (managerId == Guid.Empty)
        {
            throw new ArgumentException("O gestor é obrigatório.", nameof(managerId));
        }

        if (subordinateId == Guid.Empty)
        {
            throw new ArgumentException("O subordinado é obrigatório.", nameof(subordinateId));
        }

        if (managerId == subordinateId)
        {
            throw new ArgumentException("O gestor e o subordinado devem ser usuários diferentes.", nameof(subordinateId));
        }

        var normalizedDescription = NormalizeDescription(description);
        var normalizedCreatedAt = EnsureUtc(createdAt ?? DateTimeOffset.UtcNow);
        var normalizedDueDate = EnsureUtc(dueDate);

        if (normalizedDueDate <= normalizedCreatedAt)
        {
            throw new ArgumentException("A data limite deve ser posterior à data de criação.", nameof(dueDate));
        }

        var taskAssignment = new TaskAssignment(
            Guid.NewGuid(),
            normalizedDescription,
            normalizedDueDate,
            managerId,
            subordinateId,
            normalizedCreatedAt);

        taskAssignment.AddDomainEvent(new TaskAssignmentCreatedEvent(
            taskAssignment.Id,
            taskAssignment.ManagerId,
            taskAssignment.SubordinateId,
            taskAssignment.Description,
            taskAssignment.DueDate,
            taskAssignment.CreatedAt));

        return taskAssignment;
    }

    public void Start()
    {
        if (Status != TaskAssignmentStatus.Pending)
        {
            throw new InvalidOperationException("Somente tarefas pendentes podem ser iniciadas.");
        }

        Status = TaskAssignmentStatus.Started;
    }

    public void Complete(DateTimeOffset? completedAt = null)
    {
        if (Status == TaskAssignmentStatus.Canceled)
        {
            throw new InvalidOperationException("Tarefas canceladas não podem ser finalizadas.");
        }

        if (Status == TaskAssignmentStatus.Completed)
        {
            throw new InvalidOperationException("A tarefa já está finalizada.");
        }

        var normalizedCompletedAt = EnsureUtc(completedAt ?? DateTimeOffset.UtcNow);

        Status = TaskAssignmentStatus.Completed;
        CompletedAt = normalizedCompletedAt;

        AddDomainEvent(new TaskAssignmentCompletedEvent(
            Id,
            ManagerId,
            SubordinateId,
            Description,
            normalizedCompletedAt,
            normalizedCompletedAt));
    }

    public void Cancel()
    {
        if (Status == TaskAssignmentStatus.Completed)
        {
            throw new InvalidOperationException("Tarefas finalizadas não podem ser canceladas.");
        }

        Status = TaskAssignmentStatus.Canceled;
    }

    public bool IsOverdue(DateTimeOffset now)
    {
        return Status is not TaskAssignmentStatus.Completed and not TaskAssignmentStatus.Canceled
            && EnsureUtc(now) > DueDate;
    }

    private static string NormalizeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("A descrição é obrigatória.", nameof(description));
        }

        var trimmed = description.Trim();
        if (trimmed.Length > 2_000)
        {
            throw new ArgumentException("A descrição não pode exceder 2000 caracteres.", nameof(description));
        }

        return trimmed;
    }

    private static DateTimeOffset EnsureUtc(DateTimeOffset value)
    {
        return value.Offset == TimeSpan.Zero ? value : value.ToUniversalTime();
    }
}
