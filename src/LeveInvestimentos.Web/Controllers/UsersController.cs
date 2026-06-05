using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Web.Services.Users;
using LeveInvestimentos.Web.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeveInvestimentos.Web.Controllers;

[Authorize(Roles = "Manager")]
[Route("usuarios")]
public sealed class UsersController : Controller
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var managerId = GetCurrentUserId();
        if (managerId is null)
        {
            return Forbid();
        }

        var result = await _userManagementService.ListManagedUsersAsync(managerId.Value, cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(Array.Empty<UserListItemViewModel>());
        }

        return View(result.Value);
    }

    [HttpGet("criar")]
    public IActionResult Criar()
    {
        return View("Create", new CreateUserViewModel());
    }

    [HttpPost("criar")]
    public async Task<IActionResult> Criar(
        CreateUserViewModel model,
        CancellationToken cancellationToken = default)
    {
        var managerId = GetCurrentUserId();
        if (managerId is null)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View("Create", model);
        }

        var result = await _userManagementService.CreateAsync(managerId.Value, model, cancellationToken);

        if (result.IsFailure)
        {
            AddCreateUserError(result.Error);
            return View("Create", model);
        }

        TempData["SuccessMessage"] = "Usuario criado com sucesso. A senha temporaria foi enviada por e-mail para troca no primeiro login.";
        return RedirectToAction(nameof(Index));
    }

    private void AddCreateUserError(Error error)
    {
        if (error.Code == "Users.InvalidProfilePhoto")
        {
            ModelState.AddModelError(nameof(CreateUserViewModel.ProfilePhoto), error.Message);
            return;
        }

        ModelState.AddModelError(string.Empty, error.Message);
    }

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsed)
            ? parsed
            : null;
    }
}
