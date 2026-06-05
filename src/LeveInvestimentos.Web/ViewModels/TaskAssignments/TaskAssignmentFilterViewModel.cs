using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Web.ViewModels.TaskAssignments;

public sealed class TaskAssignmentFilterViewModel
{
    public TaskAssignmentStatus? Status { get; set; }

    public IReadOnlyCollection<SelectListItem> StatusOptions { get; set; } = Array.Empty<SelectListItem>();
}
