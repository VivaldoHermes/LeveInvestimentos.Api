using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Application.Tarefas.DTOs;
using LeveInvestimentos.Web.Services.TaskAssignments;
using LeveInvestimentos.Web.ViewModels.TaskAssignments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Web.Controllers;

[Authorize]
[Route("tarefas")]
public sealed class TaskAssignmentsController : Controller
{
    private readonly ITaskAssignmentManagementService _taskAssignmentManagementService;

    public TaskAssignmentsController(ITaskAssignmentManagementService taskAssignmentManagementService)
    {
        _taskAssignmentManagementService = taskAssignmentManagementService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        TaskAssignmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Forbid();
        }

        var result = await _taskAssignmentManagementService.BuildIndexAsync(
            currentUserId.Value,
            User.IsInRole("Manager"),
            status,
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(new TaskAssignmentIndexViewModel());
        }

        return View(result.Value);
    }

    [HttpGet("criar")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Criar(CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Forbid();
        }

        var result = await _taskAssignmentManagementService.BuildCreateAsync(
            currentUserId.Value,
            cancellationToken: cancellationToken);

        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error.Message;
            return RedirectToAction(nameof(Index));
        }

        return View("Create", result.Value);
    }

    [HttpPost("criar")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Criar(
        CreateTaskAssignmentViewModel model,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return await CreateView(currentUserId.Value, model, cancellationToken);
        }

        var result = await _taskAssignmentManagementService.CreateAsync(
            currentUserId.Value,
            model,
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return await CreateView(currentUserId.Value, model, cancellationToken);
        }

        TempData["SuccessMessage"] = "Tarefa criada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("iniciar/{id:guid}")]
    public async Task<IActionResult> Iniciar(Guid id, CancellationToken cancellationToken = default)
    {
        return await ChangeStatusAsync(
            id,
            (currentUserId, taskAssignmentId) => _taskAssignmentManagementService.StartAsync(
                currentUserId,
                taskAssignmentId,
                cancellationToken),
            "Tarefa iniciada com sucesso.");
    }

    [HttpPost("finalizar/{id:guid}")]
    public async Task<IActionResult> Finalizar(Guid id, CancellationToken cancellationToken = default)
    {
        return await ChangeStatusAsync(
            id,
            (currentUserId, taskAssignmentId) => _taskAssignmentManagementService.CompleteAsync(
                currentUserId,
                taskAssignmentId,
                cancellationToken),
            "Tarefa finalizada com sucesso.");
    }

    [HttpPost("cancelar/{id:guid}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken cancellationToken = default)
    {
        return await ChangeStatusAsync(
            id,
            (currentUserId, taskAssignmentId) => _taskAssignmentManagementService.CancelAsync(
                currentUserId,
                taskAssignmentId,
                cancellationToken),
            "Tarefa cancelada com sucesso.");
    }

    private async Task<IActionResult> CreateView(
        Guid managerId,
        CreateTaskAssignmentViewModel model,
        CancellationToken cancellationToken)
    {
        var createModelResult = await _taskAssignmentManagementService.BuildCreateAsync(
            managerId,
            model,
            cancellationToken);

        return createModelResult.IsFailure
            ? View("Create", model)
            : View("Create", createModelResult.Value);
    }

    private async Task<IActionResult> ChangeStatusAsync(
        Guid taskAssignmentId,
        Func<Guid, Guid, Task<Result<TaskAssignmentDetailsDto>>> action,
        string successMessage)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Forbid();
        }

        var result = await action(currentUserId.Value, taskAssignmentId);
        TempData[result.IsFailure ? "ErrorMessage" : "SuccessMessage"] = result.IsFailure
            ? result.Error.Message
            : successMessage;

        return RedirectToAction(nameof(Index));
    }

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsed)
            ? parsed
            : null;
    }
}
