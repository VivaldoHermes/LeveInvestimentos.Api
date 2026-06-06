using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Application.Tarefas.Commands;
using LeveInvestimentos.Core.Application.Tarefas.DTOs;
using LeveInvestimentos.Core.Application.Tarefas.Services;
using LeveInvestimentos.Core.Application.Users.Commands;
using LeveInvestimentos.Core.Application.Users.DTOs;
using LeveInvestimentos.Core.Application.Users.Services;
using LeveInvestimentos.Core.Domain.Enums;
using LeveInvestimentos.Web.Services.TaskAssignments;
using LeveInvestimentos.Tests.TestSupport;
using Xunit;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Tests.Web;

public sealed class TaskAssignmentManagementServiceTests
{
    [Fact]
    public async Task BuildIndexAsync_ForManager_MapsNamesActionsAndStatusOptions()
    {
        var tasks = new FakeTaskAssignmentApplicationService
        {
            ManagerTasks = new[]
            {
                new TaskAssignmentListItemDto(
                    Guid.NewGuid(),
                    "Tarefa pendente",
                    TestData.DueDate,
                    TaskAssignmentStatus.Pending,
                    TestData.ManagerId,
                    null,
                    TestData.SubordinateId,
                    null,
                    TestData.CreatedAt,
                    null,
                    false)
            }
        };
        var users = new FakeUserApplicationService();
        var service = new TaskAssignmentManagementService(tasks, users);

        var result = await service.BuildIndexAsync(TestData.ManagerId, isManager: true, TaskAssignmentStatus.Pending);

        result.IsSuccess.Should().BeTrue();
        result.Value.CanCreate.Should().BeTrue();
        result.Value.Filter.Status.Should().Be(TaskAssignmentStatus.Pending);
        result.Value.Filter.StatusOptions.Single(option => option.Value == TaskAssignmentStatus.Pending.ToString()).Selected.Should().BeTrue();
        result.Value.Tasks.Should().ContainSingle();
        var item = result.Value.Tasks.Single();
        item.ManagerName.Should().Be("Gestor Principal");
        item.SubordinateName.Should().Be("Subordinado Principal");
        item.CanCancel.Should().BeTrue();
        item.CanStart.Should().BeFalse();
        tasks.ListForManagerCalls.Should().Be(1);
    }

    [Fact]
    public async Task BuildIndexAsync_ForSubordinate_EnablesStartAndCompleteForPendingTask()
    {
        var tasks = new FakeTaskAssignmentApplicationService
        {
            SubordinateTasks = new[]
            {
                new TaskAssignmentListItemDto(
                    Guid.NewGuid(),
                    "Tarefa pendente",
                    TestData.DueDate,
                    TaskAssignmentStatus.Pending,
                    TestData.ManagerId,
                    null,
                    TestData.SubordinateId,
                    null,
                    TestData.CreatedAt,
                    null,
                    false)
            }
        };
        var users = new FakeUserApplicationService();
        var service = new TaskAssignmentManagementService(tasks, users);

        var result = await service.BuildIndexAsync(TestData.SubordinateId, isManager: false);

        result.IsSuccess.Should().BeTrue();
        result.Value.CanCreate.Should().BeFalse();
        var item = result.Value.Tasks.Single();
        item.ManagerName.Should().Be("Gestor Principal");
        item.SubordinateName.Should().Be("Subordinado Principal");
        item.CanStart.Should().BeTrue();
        item.CanComplete.Should().BeTrue();
        item.CanCancel.Should().BeFalse();
        tasks.ListForSubordinateCalls.Should().Be(1);
    }

    private sealed class FakeTaskAssignmentApplicationService : ITaskAssignmentService
    {
        public IReadOnlyCollection<TaskAssignmentListItemDto> ManagerTasks { get; set; } = Array.Empty<TaskAssignmentListItemDto>();
        public IReadOnlyCollection<TaskAssignmentListItemDto> SubordinateTasks { get; set; } = Array.Empty<TaskAssignmentListItemDto>();
        public int ListForManagerCalls { get; private set; }
        public int ListForSubordinateCalls { get; private set; }

        public Task<Result<TaskAssignmentDetailsDto>> CreateAsync(CreateTaskAssignmentCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<TaskAssignmentDetailsDto>.Failure(new Error("NotImplemented", "Not implemented.")));
        }

        public Task<Result<TaskAssignmentDetailsDto>> StartAsync(StartTaskAssignmentCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<TaskAssignmentDetailsDto>.Failure(new Error("NotImplemented", "Not implemented.")));
        }

        public Task<Result<TaskAssignmentDetailsDto>> CompleteAsync(CompleteTaskAssignmentCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<TaskAssignmentDetailsDto>.Failure(new Error("NotImplemented", "Not implemented.")));
        }

        public Task<Result<TaskAssignmentDetailsDto>> CancelAsync(CancelTaskAssignmentCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<TaskAssignmentDetailsDto>.Failure(new Error("NotImplemented", "Not implemented.")));
        }

        public Task<Result<IReadOnlyCollection<TaskAssignmentListItemDto>>> ListForManagerAsync(
            Guid managerId,
            TaskAssignmentStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            ListForManagerCalls++;
            return Task.FromResult(Result<IReadOnlyCollection<TaskAssignmentListItemDto>>.Success(ManagerTasks));
        }

        public Task<Result<IReadOnlyCollection<TaskAssignmentListItemDto>>> ListForSubordinateAsync(
            Guid subordinateId,
            TaskAssignmentStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            ListForSubordinateCalls++;
            return Task.FromResult(Result<IReadOnlyCollection<TaskAssignmentListItemDto>>.Success(SubordinateTasks));
        }
    }

    private sealed class FakeUserApplicationService : IUserService
    {
        public Task<Result<UserDetailsDto>> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<UserDetailsDto>.Failure(new Error("NotImplemented", "Not implemented.")));
        }

        public Task<Result<UserDetailsDto>> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var isManager = userId == TestData.ManagerId;
            return Task.FromResult(Result<UserDetailsDto>.Success(new UserDetailsDto(
                userId,
                isManager ? "Gestor Principal" : "Subordinado Principal",
                new DateOnly(1990, 1, 1),
                isManager ? "manager@example.com" : "subordinate@example.com",
                isManager ? UserRole.Manager : UserRole.Subordinate,
                isManager ? null : TestData.ManagerId,
                isManager ? null : "Gestor Principal",
                "Rua",
                "1",
                "Sao Paulo",
                "SP",
                "(11) 3333-4444",
                "(11) 99999-8888",
                string.Empty,
                false)));
        }

        public Task<Result<IReadOnlyCollection<UserListItemDto>>> ListSubordinatesAsync(Guid managerId, CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<UserListItemDto> users = new[]
            {
                new UserListItemDto(
                    TestData.SubordinateId,
                    "Subordinado Principal",
                    "subordinate@example.com",
                    UserRole.Subordinate,
                    TestData.ManagerId,
                    "Gestor Principal",
                    true)
            };

            return Task.FromResult(Result<IReadOnlyCollection<UserListItemDto>>.Success(users));
        }

        public Task<Result<IReadOnlyCollection<UserListItemDto>>> ListManagedUsersAsync(Guid managerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<IReadOnlyCollection<UserListItemDto>>.Success(Array.Empty<UserListItemDto>()));
        }
    }
}
