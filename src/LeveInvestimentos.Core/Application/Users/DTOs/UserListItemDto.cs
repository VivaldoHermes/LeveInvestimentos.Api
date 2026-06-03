using System;
using LeveInvestimentos.Core.Domain.Enums;

namespace LeveInvestimentos.Core.Application.Users.DTOs;

public sealed record UserListItemDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    Guid? ManagerId,
    string? ManagerName,
    bool MustChangePassword);
