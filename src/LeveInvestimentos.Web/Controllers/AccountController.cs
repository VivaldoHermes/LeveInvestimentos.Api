using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Application.Account;
using LeveInvestimentos.Core.Application.Account.Commands;
using LeveInvestimentos.Core.Application.Account.Services;
using LeveInvestimentos.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeveInvestimentos.Web.Controllers;

[Route("account")]
public sealed class AccountController : Controller
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ViewData["ReturnUrl"] = returnUrl;

        var result = await _accountService.LoginAsync(
            new LoginCommand(
                model.Email,
                model.Password,
                model.RememberMe),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(model);
        }

        if (result.Value.RequiresPasswordChange)
        {
            return RedirectToAction(nameof(ChangePassword), new { returnUrl });
        }

        return RedirectToLocal(returnUrl);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _accountService.SignOutAsync(cancellationToken);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("access-denied")]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet("change-password")]
    [Authorize]
    public IActionResult ChangePassword(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new ChangePasswordViewModel());
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordViewModel model,
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = GetCurrentUserId();
        if (userId is null)
        {
            await _accountService.SignOutAsync(cancellationToken);
            return RedirectToAction(nameof(Login));
        }

        var result = await _accountService.ChangePasswordAsync(
            new ChangePasswordCommand(
                userId.Value,
                model.CurrentPassword,
                model.NewPassword),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == AccountErrors.UserNotFound)
            {
                await _accountService.SignOutAsync(cancellationToken);
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = "Senha alterada com sucesso.";

        return RedirectToLocal(returnUrl);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsed)
            ? parsed
            : null;
    }
}
