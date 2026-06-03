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
            throw new ArgumentException("Manager is required.", nameof(managerId));
        }

        if (subordinateId == Guid.Empty)
        {
            throw new ArgumentException("Subordinate is required.", nameof(subordinateId));
        }

        if (managerId == subordinateId)
        {
            throw new ArgumentException("Manager and subordinate must be different users.", nameof(subordinateId));
        }

        var normalizedDescription = NormalizeDescription(description);
        var normalizedCreatedAt = EnsureUtc(createdAt ?? DateTimeOffset.UtcNow);
        var normalizedDueDate = EnsureUtc(dueDate);

        if (normalizedDueDate <= normalizedCreatedAt)
        {
            throw new ArgumentException("Due date must be later than creation date.", nameof(dueDate));
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
            throw new InvalidOperationException("Only pending task assignments can be started.");
        }

        Status = TaskAssignmentStatus.Started;
    }

    public void Complete(DateTimeOffset? completedAt = null)
    {
        if (Status == TaskAssignmentStatus.Canceled)
        {
            throw new InvalidOperationException("Canceled task assignments cannot be completed.");
        }

        if (Status == TaskAssignmentStatus.Completed)
        {
            throw new InvalidOperationException("Task assignment is already completed.");
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
            throw new InvalidOperationException("Completed task assignments cannot be canceled.");
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
            throw new ArgumentException("Description is required.", nameof(description));
        }

        var trimmed = description.Trim();
        if (trimmed.Length > 2_000)
        {
            throw new ArgumentException("Description cannot exceed 2000 characters.", nameof(description));
        }

        return trimmed;
    }

    private static DateTimeOffset EnsureUtc(DateTimeOffset value)
    {
        return value.Offset == TimeSpan.Zero ? value : value.ToUniversalTime();
    }
}
