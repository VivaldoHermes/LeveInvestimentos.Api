using System;
using TaskAssignmentStatus = LeveInvestimentos.Core.Domain.Enums.TaskStatus;

namespace LeveInvestimentos.Web.ViewModels.Home;

public sealed class HomeDashboardTaskViewModel
{
    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public string SubordinateName { get; set; } = string.Empty;

    public DateTimeOffset DueDate { get; set; }

    public TaskAssignmentStatus Status { get; set; }

    public bool IsOverdue { get; set; }

    public string StatusDisplayName => Status switch
    {
        TaskAssignmentStatus.Pending => "Pendente",
        TaskAssignmentStatus.Started => "Em andamento",
        TaskAssignmentStatus.Completed => "Concluida",
        TaskAssignmentStatus.Canceled => "Cancelada",
        _ => Status.ToString()
    };
}
