using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LeveInvestimentos.Infrastructure.Persistence.Repositories;
using LeveInvestimentos.Tests.TestSupport;
using Xunit;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Tests.Integration;

public sealed class TaskAssignmentRepositoryTests
{
    [Fact]
    public async Task ListByManagerIdAsync_FiltersByManagerStatusAndOrdersByDueDate()
    {
        using var factory = new SqliteDbContextFactory();
        await using var context = factory.CreateContext();
        var repository = new TaskAssignmentRepository(context);
        context.Users.AddRange(
            TestData.Manager(),
            TestData.Manager(TestData.OtherManagerId, "other-manager@example.com"),
            TestData.Subordinate());
        await context.SaveChangesAsync();

        var later = TestData.TaskAssignment(dueDate: TestData.DueDate.AddDays(2));
        var earlier = TestData.TaskAssignment(dueDate: TestData.DueDate.AddDays(1));
        var otherManager = TestData.TaskAssignment(managerId: TestData.OtherManagerId, dueDate: TestData.DueDate.AddHours(1));
        var completed = TestData.TaskAssignment(dueDate: TestData.DueDate.AddDays(3));
        completed.Complete(TestData.CreatedAt.AddHours(1));
        context.TaskAssignments.AddRange(later, earlier, otherManager, completed);
        await context.SaveChangesAsync();

        var result = await repository.ListByManagerIdAsync(TestData.ManagerId, TaskAssignmentStatus.Pending);

        result.Should().HaveCount(2);
        result.Select(task => task.Id).Should().Equal(earlier.Id, later.Id);
        result.Should().OnlyContain(task => task.ManagerId == TestData.ManagerId && task.Status == TaskAssignmentStatus.Pending);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTrackedTask()
    {
        using var factory = new SqliteDbContextFactory();
        await using var context = factory.CreateContext();
        var repository = new TaskAssignmentRepository(context);
        context.Users.AddRange(TestData.Manager(), TestData.Subordinate());
        await context.SaveChangesAsync();

        var taskAssignment = TestData.TaskAssignment();
        context.TaskAssignments.Add(taskAssignment);
        await context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(taskAssignment.Id);

        result.Should().NotBeNull();
        result!.Description.Should().Be(taskAssignment.Description);
    }
}
