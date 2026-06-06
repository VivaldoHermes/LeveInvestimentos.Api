using System;
using LeveInvestimentos.Core.Domain.Entities;
using LeveInvestimentos.Core.Domain.Enums;
using LeveInvestimentos.Core.Domain.ValueObjects;

namespace LeveInvestimentos.Tests.TestSupport;

internal static class TestData
{
    public static readonly Guid ManagerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid SubordinateId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid OtherManagerId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly DateTimeOffset CreatedAt = new(2026, 6, 6, 12, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset DueDate = CreatedAt.AddDays(1);

    public static Address Address()
    {
        return new Address("Rua Central", "100", "Sao Paulo", "sp");
    }

    public static User Manager(Guid? id = null, string email = "manager@example.com")
    {
        return User.Create(
            "Gestor Principal",
            new DateOnly(1990, 1, 1),
            email,
            Address(),
            "(11) 3333-4444",
            "(11) 99999-8888",
            string.Empty,
            UserRole.Manager,
            new DateOnly(2026, 6, 6),
            null,
            id ?? ManagerId);
    }

    public static User Subordinate(Guid? id = null, Guid? managerId = null, string email = "subordinate@example.com")
    {
        return User.Create(
            "Subordinado Principal",
            new DateOnly(1995, 2, 2),
            email,
            Address(),
            "(11) 3333-5555",
            "(11) 99999-7777",
            "uploads/foto.jpg",
            UserRole.Subordinate,
            new DateOnly(2026, 6, 6),
            managerId ?? ManagerId,
            id ?? SubordinateId);
    }

    public static TaskAssignment TaskAssignment(
        Guid? managerId = null,
        Guid? subordinateId = null,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? dueDate = null)
    {
        return Core.Domain.Entities.TaskAssignment.Create(
            managerId ?? ManagerId,
            subordinateId ?? SubordinateId,
            "Preparar relatorio mensal",
            dueDate ?? DueDate,
            createdAt ?? CreatedAt);
    }
}
