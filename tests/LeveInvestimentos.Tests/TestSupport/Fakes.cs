using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Core.Application.Account.DTOs;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Domain.Entities;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Tests.TestSupport;

internal sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private readonly HashSet<string> _existingEmails = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<User> AddedUsers => _users;
    public string? LastPassword { get; private set; }

    public void AddExisting(User user)
    {
        _users.Add(user);
        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            _existingEmails.Add(user.Email);
        }
    }

    public void MarkEmailAsExisting(string email)
    {
        _existingEmails.Add(email);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(user => user.Id == id));
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_existingEmails.Contains(email));
    }

    public Task<IReadOnlyCollection<User>> ListByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<User>>(_users.Where(user => user.ManagerId == managerId).ToArray());
    }

    public Task<IReadOnlyCollection<User>> ListManagedUsersAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<User>>(_users.Where(user => user.Id == managerId || user.ManagerId == managerId).ToArray());
    }

    public Task AddAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        _users.Add(user);
        _existingEmails.Add(user.Email!);
        LastPassword = password;
        return Task.CompletedTask;
    }
}

internal sealed class FakeTaskAssignmentRepository : ITaskAssignmentRepository
{
    private readonly List<TaskAssignment> _tasks = new();

    public IReadOnlyCollection<TaskAssignment> AddedTasks => _tasks;

    public void AddExisting(TaskAssignment taskAssignment)
    {
        _tasks.Add(taskAssignment);
    }

    public Task<TaskAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_tasks.FirstOrDefault(taskAssignment => taskAssignment.Id == id));
    }

    public Task AddAsync(TaskAssignment taskAssignment, CancellationToken cancellationToken = default)
    {
        _tasks.Add(taskAssignment);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<TaskAssignment>> ListByManagerIdAsync(
        Guid managerId,
        TaskAssignmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<TaskAssignment>>(
            ApplyStatus(_tasks.Where(taskAssignment => taskAssignment.ManagerId == managerId), status).ToArray());
    }

    public Task<IReadOnlyCollection<TaskAssignment>> ListBySubordinateIdAsync(
        Guid subordinateId,
        TaskAssignmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<TaskAssignment>>(
            ApplyStatus(_tasks.Where(taskAssignment => taskAssignment.SubordinateId == subordinateId), status).ToArray());
    }

    private static IEnumerable<TaskAssignment> ApplyStatus(IEnumerable<TaskAssignment> query, TaskAssignmentStatus? status)
    {
        return status is null ? query : query.Where(taskAssignment => taskAssignment.Status == status);
    }
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }
}

internal sealed class FakePasswordGenerator : IPasswordGenerator
{
    public string Password { get; set; } = "Senha!12345";

    public string Generate()
    {
        return Password;
    }
}

internal sealed class FakeEmailOutbox : IEmailOutbox
{
    public List<(string To, string Subject, string Body)> Messages { get; } = new();

    public Task EnqueueAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        Messages.Add((to, subject, body));
        return Task.CompletedTask;
    }
}

internal sealed class FakeAccountIdentityGateway : IAccountIdentityGateway
{
    public string? LastEmail { get; private set; }
    public string? LastPassword { get; private set; }
    public bool LastRememberMe { get; private set; }
    public Guid LastUserId { get; private set; }
    public int SignInCalls { get; private set; }
    public int SignOutCalls { get; private set; }
    public int ChangePasswordCalls { get; private set; }
    public int MustChangePasswordCalls { get; private set; }

    public Result<AccountSignInResult> SignInResult { get; set; } =
        Result<AccountSignInResult>.Success(new AccountSignInResult(TestData.SubordinateId, false));

    public Result ChangePasswordResult { get; set; } = Result.Success();

    public Result<bool> MustChangePasswordResult { get; set; } = Result<bool>.Success(false);

    public Task<Result<AccountSignInResult>> SignInAsync(
        string email,
        string password,
        bool rememberMe,
        CancellationToken cancellationToken = default)
    {
        SignInCalls++;
        LastEmail = email;
        LastPassword = password;
        LastRememberMe = rememberMe;
        return Task.FromResult(SignInResult);
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        SignOutCalls++;
        return Task.CompletedTask;
    }

    public Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        ChangePasswordCalls++;
        LastUserId = userId;
        LastPassword = newPassword;
        return Task.FromResult(ChangePasswordResult);
    }

    public Task<Result<bool>> MustChangePasswordAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        MustChangePasswordCalls++;
        LastUserId = userId;
        return Task.FromResult(MustChangePasswordResult);
    }
}
