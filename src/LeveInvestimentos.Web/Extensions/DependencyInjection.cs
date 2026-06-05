using LeveInvestimentos.Web.Filters;
using LeveInvestimentos.Web.Services.Home;
using LeveInvestimentos.Web.Services.TaskAssignments;
using LeveInvestimentos.Web.Services.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace LeveInvestimentos.Web.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddWebPresentation(this IServiceCollection services)
    {
        services.AddScoped<RequirePasswordChangeFilter>();
        services.AddScoped<IHomeDashboardService, HomeDashboardService>();
        services.AddScoped<ITaskAssignmentManagementService, TaskAssignmentManagementService>();
        services.AddScoped<IUserManagementService, UserManagementService>();

        // Require a valid anti-forgery token on every state-changing request
        // (POST/PUT/PATCH/DELETE). Razor forms emit the token automatically.
        services.AddControllersWithViews(options =>
        {
            options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            options.Filters.AddService<RequirePasswordChangeFilter>();
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.LoginPath = "/account/login";
            options.LogoutPath = "/account/logout";
            options.AccessDeniedPath = "/account/access-denied";
        });

        return services;
    }
}
