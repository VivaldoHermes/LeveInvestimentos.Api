using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Domain.Events;

namespace LeveInvestimentos.Core.Application.Notifications;

public sealed class TaskAssignmentCreatedEmailHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;

    public TaskAssignmentCreatedEmailHandler(IUserRepository userRepository, IEmailSender emailSender)
    {
        _userRepository = userRepository;
        _emailSender = emailSender;
    }

    public async Task<Result> HandleAsync(
        TaskAssignmentCreatedEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var subordinate = await _userRepository.GetByIdAsync(domainEvent.SubordinateId, cancellationToken);
        if (subordinate?.Email is null)
        {
            return Result.Failure(new Error("Notifications.SubordinateEmailNotFound", "Subordinate e-mail was not found."));
        }

        var body = $"Nova tarefa atribuida: {domainEvent.Description}. Prazo: {domainEvent.DueDate:dd/MM/yyyy HH:mm}.";
        await _emailSender.SendAsync(subordinate.Email, "Nova tarefa atribuida", body, cancellationToken);

        return Result.Success();
    }
}
