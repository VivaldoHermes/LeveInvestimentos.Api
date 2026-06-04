using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Core.Application.Notifications;
using LeveInvestimentos.Infrastructure.DomainEvents;
using LeveInvestimentos.Infrastructure.Outbox;
using LeveInvestimentos.Infrastructure.Persistence;
using LeveInvestimentos.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LeveInvestimentos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITaskAssignmentRepository, TaskAssignmentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEmailOutbox, EmailOutbox>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<TaskAssignmentCreatedEmailHandler>();
        services.AddScoped<TaskAssignmentCompletedEmailHandler>();

        return services;
    }
}
