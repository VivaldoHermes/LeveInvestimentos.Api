using System;
using System.Collections.Generic;

namespace LeveInvestimentos.Web.ViewModels.TaskAssignments;

public sealed class TaskAssignmentIndexViewModel
{
    public TaskAssignmentFilterViewModel Filter { get; set; } = new();

    public IReadOnlyCollection<TaskAssignmentListItemViewModel> Tasks { get; set; } = Array.Empty<TaskAssignmentListItemViewModel>();

    public bool CanCreate { get; set; }
}
