using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Domain.Events;

namespace LeveInvestimentos.Core.Application.Notifications;

public sealed class TaskAssignmentCompletedEmailHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailOutbox _emailOutbox;

    public TaskAssignmentCompletedEmailHandler(IUserRepository userRepository, IEmailOutbox emailOutbox)
    {
        _userRepository = userRepository;
        _emailOutbox = emailOutbox;
    }

    public async Task<Result> HandleAsync(
        TaskAssignmentCompletedEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var manager = await _userRepository.GetByIdAsync(domainEvent.ManagerId, cancellationToken);
        if (manager?.Email is null)
        {
            return Result.Failure(new Error("Notifications.ManagerEmailNotFound", "Manager e-mail was not found."));
        }

        var body = $"Tarefa finalizada: {domainEvent.Description}. Finalizada em: {domainEvent.CompletedAt:dd/MM/yyyy HH:mm}.";
        await _emailOutbox.EnqueueAsync(manager.Email, "Tarefa finalizada", body, cancellationToken);

        return Result.Success();
    }
}
