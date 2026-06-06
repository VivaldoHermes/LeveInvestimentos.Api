using System;
using FluentAssertions;
using LeveInvestimentos.Core.Domain.Events;
using LeveInvestimentos.Tests.TestSupport;
using Xunit;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Tests.Domain;

public sealed class TaskAssignmentTests
{
    [Fact]
    public void Create_WithValidData_CreatesPendingTaskAndCreatedEvent()
    {
        var taskAssignment = TestData.TaskAssignment();

        taskAssignment.Id.Should().NotBeEmpty();
        taskAssignment.Description.Should().Be("Preparar relatorio mensal");
        taskAssignment.Status.Should().Be(TaskAssignmentStatus.Pending);
        taskAssignment.CreatedAt.Should().Be(TestData.CreatedAt);
        taskAssignment.DueDate.Should().Be(TestData.DueDate);
        taskAssignment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskAssignmentCreatedEvent>();
    }

    [Fact]
    public void Create_WithNonUtcDates_StoresUtcDates()
    {
        var createdAt = new DateTimeOffset(2026, 6, 6, 9, 0, 0, TimeSpan.FromHours(-3));
        var dueDate = createdAt.AddHours(1);

        var taskAssignment = TestData.TaskAssignment(createdAt: createdAt, dueDate: dueDate);

        taskAssignment.CreatedAt.Offset.Should().Be(TimeSpan.Zero);
        taskAssignment.DueDate.Offset.Should().Be(TimeSpan.Zero);
        taskAssignment.CreatedAt.Should().Be(createdAt.ToUniversalTime());
        taskAssignment.DueDate.Should().Be(dueDate.ToUniversalTime());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankDescription_Throws(string description)
    {
        Action act = () => LeveInvestimentos.Core.Domain.Entities.TaskAssignment.Create(
            TestData.ManagerId,
            TestData.SubordinateId,
            description,
            TestData.DueDate,
            TestData.CreatedAt);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("description");
    }

    [Fact]
    public void Create_WithSameManagerAndSubordinate_Throws()
    {
        Action act = () => LeveInvestimentos.Core.Domain.Entities.TaskAssignment.Create(
            TestData.ManagerId,
            TestData.ManagerId,
            "Descricao",
            TestData.DueDate,
            TestData.CreatedAt);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("subordinateId");
    }

    [Fact]
    public void Start_FromPending_ChangesStatusToStarted()
    {
        var taskAssignment = TestData.TaskAssignment();

        taskAssignment.Start();

        taskAssignment.Status.Should().Be(TaskAssignmentStatus.Started);
    }

    [Fact]
    public void Start_FromCompleted_Throws()
    {
        var taskAssignment = TestData.TaskAssignment();
        taskAssignment.Complete(TestData.CreatedAt.AddHours(2));

        Action act = taskAssignment.Start;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Complete_FromStarted_SetsStatusCompletedAtAndEvent()
    {
        var completedAt = TestData.CreatedAt.AddHours(3);
        var taskAssignment = TestData.TaskAssignment();
        taskAssignment.ClearDomainEvents();
        taskAssignment.Start();

        taskAssignment.Complete(completedAt);

        taskAssignment.Status.Should().Be(TaskAssignmentStatus.Completed);
        taskAssignment.CompletedAt.Should().Be(completedAt);
        taskAssignment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskAssignmentCompletedEvent>();
    }

    [Fact]
    public void Complete_WhenCanceled_ThrowsAndDoesNotSetCompletedAt()
    {
        var taskAssignment = TestData.TaskAssignment();
        taskAssignment.Cancel();

        Action act = () => taskAssignment.Complete(TestData.CreatedAt.AddHours(1));

        act.Should().Throw<InvalidOperationException>();
        taskAssignment.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Cancel_WhenCompleted_Throws()
    {
        var taskAssignment = TestData.TaskAssignment();
        taskAssignment.Complete(TestData.CreatedAt.AddHours(1));

        Action act = taskAssignment.Cancel;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IsOverdue_ReturnsTrueOnlyForOpenTasksPastDueDate()
    {
        var taskAssignment = TestData.TaskAssignment();

        taskAssignment.IsOverdue(TestData.DueDate.AddTicks(1)).Should().BeTrue();

        taskAssignment.Complete(TestData.DueDate.AddHours(1));
        taskAssignment.IsOverdue(TestData.DueDate.AddDays(1)).Should().BeFalse();
    }
}
