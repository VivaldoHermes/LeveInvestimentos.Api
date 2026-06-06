using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LeveInvestimentos.Core.Application.Tarefas.Commands;
using LeveInvestimentos.Core.Application.Tarefas.Services;
using LeveInvestimentos.Tests.TestSupport;
using Xunit;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Tests.Application;

public sealed class TaskAssignmentServiceTests
{
    private readonly FakeTaskAssignmentRepository _tasks = new();
    private readonly FakeUserRepository _users = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task CreateAsync_WithManagedSubordinate_AddsTaskAndSaves()
    {
        _users.AddExisting(TestData.Manager());
        _users.AddExisting(TestData.Subordinate());
        var service = CreateService();
        var command = new CreateTaskAssignmentCommand(
            TestData.ManagerId,
            TestData.SubordinateId,
            "Nova tarefa",
            DateTimeOffset.UtcNow.AddDays(1));

        var result = await service.CreateAsync(command);

        result.IsSuccess.Should().BeTrue();
        _tasks.AddedTasks.Should().ContainSingle();
        _unitOfWork.SaveChangesCalls.Should().Be(1);
        result.Value.ManagerName.Should().Be("Gestor Principal");
        result.Value.SubordinateName.Should().Be("Subordinado Principal");
    }

    [Fact]
    public async Task CreateAsync_WhenSubordinateBelongsToAnotherManager_ReturnsFailureAndDoesNotSave()
    {
        _users.AddExisting(TestData.Manager());
        _users.AddExisting(TestData.Subordinate(managerId: TestData.OtherManagerId));
        var service = CreateService();
        var command = new CreateTaskAssignmentCommand(
            TestData.ManagerId,
            TestData.SubordinateId,
            "Nova tarefa",
            DateTimeOffset.UtcNow.AddDays(1));

        var result = await service.CreateAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskAssignments.SubordinateNotManaged");
        _tasks.AddedTasks.Should().BeEmpty();
        _unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task StartAsync_WhenCurrentUserIsNotResponsible_ReturnsForbiddenAndDoesNotSave()
    {
        var taskAssignment = TestData.TaskAssignment();
        _tasks.AddExisting(taskAssignment);
        var service = CreateService();

        var result = await service.StartAsync(new StartTaskAssignmentCommand(taskAssignment.Id, TestData.OtherManagerId));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskAssignments.Forbidden");
        taskAssignment.Status.Should().Be(TaskAssignmentStatus.Pending);
        _unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task CompleteAsync_ByResponsibleSubordinate_CompletesAndSaves()
    {
        var taskAssignment = TestData.TaskAssignment();
        _tasks.AddExisting(taskAssignment);
        var service = CreateService();

        var result = await service.CompleteAsync(new CompleteTaskAssignmentCommand(taskAssignment.Id, TestData.SubordinateId));

        result.IsSuccess.Should().BeTrue();
        taskAssignment.Status.Should().Be(TaskAssignmentStatus.Completed);
        taskAssignment.CompletedAt.Should().NotBeNull();
        _unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact]
    public async Task CancelAsync_ByManager_CancelsAndSaves()
    {
        var taskAssignment = TestData.TaskAssignment();
        _tasks.AddExisting(taskAssignment);
        var service = CreateService();

        var result = await service.CancelAsync(new CancelTaskAssignmentCommand(taskAssignment.Id, TestData.ManagerId));

        result.IsSuccess.Should().BeTrue();
        taskAssignment.Status.Should().Be(TaskAssignmentStatus.Canceled);
        _unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact]
    public async Task ListForSubordinateAsync_WithStatus_ReturnsFilteredTasks()
    {
        var pending = TestData.TaskAssignment();
        var completed = TestData.TaskAssignment(dueDate: TestData.DueDate.AddDays(1));
        completed.Complete(TestData.CreatedAt.AddHours(1));
        _tasks.AddExisting(pending);
        _tasks.AddExisting(completed);
        var service = CreateService();

        var result = await service.ListForSubordinateAsync(TestData.SubordinateId, TaskAssignmentStatus.Completed);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value.Single().Status.Should().Be(TaskAssignmentStatus.Completed);
    }

    private TaskAssignmentService CreateService()
    {
        return new TaskAssignmentService(_tasks, _users, _unitOfWork);
    }
}
