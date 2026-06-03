using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Core.Application.Notifications;
using LeveInvestimentos.Core.Domain.Events;

namespace LeveInvestimentos.Infrastructure.DomainEvents;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly TaskAssignmentCreatedEmailHandler _taskAssignmentCreatedEmailHandler;
    private readonly TaskAssignmentCompletedEmailHandler _taskAssignmentCompletedEmailHandler;

    public DomainEventDispatcher(
        TaskAssignmentCreatedEmailHandler taskAssignmentCreatedEmailHandler,
        TaskAssignmentCompletedEmailHandler taskAssignmentCompletedEmailHandler)
    {
        _taskAssignmentCreatedEmailHandler = taskAssignmentCreatedEmailHandler;
        _taskAssignmentCompletedEmailHandler = taskAssignmentCompletedEmailHandler;
    }

    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            switch (domainEvent)
            {
                case TaskAssignmentCreatedEvent taskAssignmentCreatedEvent:
                    var createdResult = await _taskAssignmentCreatedEmailHandler.HandleAsync(
                        taskAssignmentCreatedEvent,
                        cancellationToken);
                    EnsureSuccess(createdResult.IsSuccess, createdResult.Error.Message);
                    break;

                case TaskAssignmentCompletedEvent taskAssignmentCompletedEvent:
                    var completedResult = await _taskAssignmentCompletedEmailHandler.HandleAsync(
                        taskAssignmentCompletedEvent,
                        cancellationToken);
                    EnsureSuccess(completedResult.IsSuccess, completedResult.Error.Message);
                    break;
            }
        }
    }

    private static void EnsureSuccess(bool isSuccess, string errorMessage)
    {
        if (!isSuccess)
        {
            throw new InvalidOperationException(errorMessage);
        }
    }
}
