using System;
using LeveInvestimentos.Core.Abstractions;
using LeveInvestimentos.Core.Application.Notifications;
using LeveInvestimentos.Core.Domain.Entities;
using LeveInvestimentos.Infrastructure.BackgroundServices;
using LeveInvestimentos.Infrastructure.DomainEvents;
using LeveInvestimentos.Infrastructure.Email;
using LeveInvestimentos.Infrastructure.Identity;
using LeveInvestimentos.Infrastructure.Files;
using LeveInvestimentos.Infrastructure.Outbox;
using LeveInvestimentos.Infrastructure.Persistence;
using LeveInvestimentos.Infrastructure.Persistence.Repositories;
using LeveInvestimentos.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LeveInvestimentos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        AddPersistence(services, configuration);
        AddIdentity(services);
        AddEmail(services, configuration);
        services.AddLocalFileStorage(configuration, environment.ContentRootPath);
        AddDomainServices(services);
        services.AddHostedService<OutboxDispatcherService>();

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
    }

    private static void AddIdentity(IServiceCollection services)
    {
        services
            .AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddSingleton<IPasswordGenerator, PasswordGenerator>();
        services.AddScoped<DatabaseSeeder>();
    }

    private static void AddEmail(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddScoped<IEmailSender, SmtpEmailSender>();
    }

    private static void AddDomainServices(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITaskAssignmentRepository, TaskAssignmentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAccountIdentityGateway, AccountIdentityGateway>();
        services.AddScoped<IEmailOutbox, EmailOutbox>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<TaskAssignmentCreatedEmailHandler>();
        services.AddScoped<TaskAssignmentCompletedEmailHandler>();
    }
}
