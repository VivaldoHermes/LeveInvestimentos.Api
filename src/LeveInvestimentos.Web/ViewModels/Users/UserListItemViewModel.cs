using System;
using LeveInvestimentos.Core.Domain.Enums;

namespace LeveInvestimentos.Web.ViewModels.Users;

public sealed class UserListItemViewModel
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public string? ManagerName { get; set; }

    public bool MustChangePassword { get; set; }

    public string RoleDisplayName => Role == UserRole.Manager ? "Gestor" : "Subordinado";
}
