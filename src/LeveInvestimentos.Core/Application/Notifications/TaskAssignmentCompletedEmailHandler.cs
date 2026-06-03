using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Domain.Events;

namespace LeveInvestimentos.Core.Application.Notifications;

public sealed class TaskAssignmentCompletedEmailHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;

    public TaskAssignmentCompletedEmailHandler(IUserRepository userRepository, IEmailSender emailSender)
    {
        _userRepository = userRepository;
        _emailSender = emailSender;
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
        await _emailSender.SendAsync(manager.Email, "Tarefa finalizada", body, cancellationToken);

        return Result.Success();
    }
}
