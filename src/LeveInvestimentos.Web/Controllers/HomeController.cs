using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Web.Services.Home;
using LeveInvestimentos.Web.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeveInvestimentos.Web.Controllers;

[Authorize]
public sealed class HomeController : Controller
{
    private readonly IHomeDashboardService _homeDashboardService;

    public HomeController(IHomeDashboardService homeDashboardService)
    {
        _homeDashboardService = homeDashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        if (!User.IsInRole("Manager"))
        {
            return RedirectToAction("Index", "TaskAssignments");
        }

        var managerId = GetCurrentUserId();
        if (managerId is null)
        {
            return Forbid();
        }

        var result = await _homeDashboardService.BuildForManagerAsync(managerId.Value, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error.Message;
            return View(new HomeDashboardViewModel());
        }

        return View(result.Value);
    }

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsed)
            ? parsed
            : null;
    }
}
