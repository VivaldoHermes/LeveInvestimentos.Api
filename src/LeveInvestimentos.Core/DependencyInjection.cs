using LeveInvestimentos.Core.Application.Tarefas.Services;
using LeveInvestimentos.Core.Application.Users.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LeveInvestimentos.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITaskAssignmentService, TaskAssignmentService>();

        return services;
    }
}
