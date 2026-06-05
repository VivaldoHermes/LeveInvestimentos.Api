using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Domain.Enums;
using LeveInvestimentos.Web.Services.TaskAssignments;
using LeveInvestimentos.Web.Services.Users;
using LeveInvestimentos.Web.ViewModels.Home;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Web.Services.Home;

public sealed class HomeDashboardService : IHomeDashboardService
{
    private readonly ITaskAssignmentManagementService _taskAssignmentManagementService;
    private readonly IUserManagementService _userManagementService;

    public HomeDashboardService(
        ITaskAssignmentManagementService taskAssignmentManagementService,
        IUserManagementService userManagementService)
    {
        _taskAssignmentManagementService = taskAssignmentManagementService;
        _userManagementService = userManagementService;
    }

    public async Task<Result<HomeDashboardViewModel>> BuildForManagerAsync(
        Guid managerId,
        CancellationToken cancellationToken = default)
    {
        var tasksResult = await _taskAssignmentManagementService.BuildIndexAsync(
            managerId,
            isManager: true,
            cancellationToken: cancellationToken);

        if (tasksResult.IsFailure)
        {
            return Result<HomeDashboardViewModel>.Failure(tasksResult.Error);
        }

        var usersResult = await _userManagementService.ListManagedUsersAsync(managerId, cancellationToken);
        if (usersResult.IsFailure)
        {
            return Result<HomeDashboardViewModel>.Failure(usersResult.Error);
        }

        var tasks = tasksResult.Value.Tasks;
        var users = usersResult.Value;
        var dueSoonLimit = DateTimeOffset.UtcNow.AddDays(7);
        var activeTasks = tasks
            .Where(task => task.Status is TaskAssignmentStatus.Pending or TaskAssignmentStatus.Started)
            .ToArray();

        var model = new HomeDashboardViewModel
        {
            TotalTasks = tasks.Count,
            PendingTasks = tasks.Count(task => task.Status == TaskAssignmentStatus.Pending),
            StartedTasks = tasks.Count(task => task.Status == TaskAssignmentStatus.Started),
            CompletedTasks = tasks.Count(task => task.Status == TaskAssignmentStatus.Completed),
            CanceledTasks = tasks.Count(task => task.Status == TaskAssignmentStatus.Canceled),
            OverdueTasks = tasks.Count(task => task.IsOverdue),
            DueSoonTasks = activeTasks.Count(task => task.DueDate <= dueSoonLimit),
            ManagedUsers = users.Count,
            Subordinates = users.Count(user => user.Role == UserRole.Subordinate),
            PendingPasswordChanges = users.Count(user => user.MustChangePassword),
            DueSoonLimit = dueSoonLimit,
            PriorityTasks = activeTasks
                .OrderByDescending(task => task.IsOverdue)
                .ThenBy(task => task.DueDate)
                .Take(5)
                .Select(task => new HomeDashboardTaskViewModel
                {
                    Id = task.Id,
                    Description = task.Description,
                    SubordinateName = task.SubordinateName,
                    DueDate = task.DueDate,
                    Status = task.Status,
                    IsOverdue = task.IsOverdue
                })
                .ToArray()
        };

        return Result<HomeDashboardViewModel>.Success(model);
    }
}
