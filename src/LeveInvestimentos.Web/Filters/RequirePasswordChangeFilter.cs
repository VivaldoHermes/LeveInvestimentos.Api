using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Application.Account.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LeveInvestimentos.Web.Filters;

public sealed class RequirePasswordChangeFilter : IAsyncActionFilter
{
    private readonly IAccountService _accountService;

    public RequirePasswordChangeFilter(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true || IsAccountException(context))
        {
            await next();
            return;
        }

        var userId = GetCurrentUserId(context);
        if (userId is null)
        {
            await _accountService.SignOutAsync(context.HttpContext.RequestAborted);
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        var result = await _accountService.MustChangePasswordAsync(
            userId.Value,
            context.HttpContext.RequestAborted);

        if (result.IsFailure)
        {
            await _accountService.SignOutAsync(context.HttpContext.RequestAborted);
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        if (result.Value)
        {
            var request = context.HttpContext.Request;
            var returnUrl = request.PathBase + request.Path + request.QueryString;

            context.Result = new RedirectToActionResult(
                "ChangePassword",
                "Account",
                new { returnUrl });
            return;
        }

        await next();
    }

    private static bool IsAccountException(ActionExecutingContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
        {
            return false;
        }

        return descriptor.ControllerName == "Account"
            && (descriptor.ActionName == "ChangePassword"
                || descriptor.ActionName == "Logout"
                || descriptor.ActionName == "AccessDenied");
    }

    private static Guid? GetCurrentUserId(ActionExecutingContext context)
    {
        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsed)
            ? parsed
            : null;
    }
}
