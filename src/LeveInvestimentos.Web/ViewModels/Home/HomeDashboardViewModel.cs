using System;
using System.Collections.Generic;

namespace LeveInvestimentos.Web.ViewModels.Home;

public sealed class HomeDashboardViewModel
{
    public int TotalTasks { get; set; }

    public int PendingTasks { get; set; }

    public int StartedTasks { get; set; }

    public int CompletedTasks { get; set; }

    public int CanceledTasks { get; set; }

    public int OverdueTasks { get; set; }

    public int DueSoonTasks { get; set; }

    public int ManagedUsers { get; set; }

    public int Subordinates { get; set; }

    public int PendingPasswordChanges { get; set; }

    public DateTimeOffset DueSoonLimit { get; set; }

    public IReadOnlyCollection<HomeDashboardTaskViewModel> PriorityTasks { get; set; } = Array.Empty<HomeDashboardTaskViewModel>();
}
