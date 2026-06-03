using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Domain.Entities;
using LeveInvestimentos.Core.Domain.Enums;
using LeveInvestimentos.Core.Domain.ValueObjects;
using LeveInvestimentos.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace LeveInvestimentos.Infrastructure.Persistence.Seed;

public sealed class DatabaseSeeder
{
    public const string DefaultManagerEmail = "ti@leveinvestimentos.com.br";
    public const string DefaultManagerPassword = "teste123";

    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<User> _userManager;

    public DatabaseSeeder(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<User> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureRoleAsync(IdentityRoleNames.Manager, cancellationToken);
        await EnsureRoleAsync(IdentityRoleNames.Subordinate, cancellationToken);
        await EnsureDefaultManagerAsync(cancellationToken);
    }

    private async Task EnsureRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (await _roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(BuildIdentityErrorMessage(result));
        }
    }

    private async Task EnsureDefaultManagerAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var manager = await _userManager.FindByEmailAsync(DefaultManagerEmail);
        if (manager is null)
        {
            manager = User.Create(
                "TI LEVE Investimentos",
                new DateOnly(1990, 1, 1),
                DefaultManagerEmail,
                new Address("Avenida Paulista", "1000", "Sao Paulo", "SP"),
                "(11) 3000-0000",
                "(11) 90000-0000",
                "/uploads/default-manager.png",
                UserRole.Manager,
                DateOnly.FromDateTime(DateTime.UtcNow));

            manager.MarkPasswordAsChanged();
            manager.EmailConfirmed = true;

            var createResult = await _userManager.CreateAsync(manager, DefaultManagerPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(BuildIdentityErrorMessage(createResult));
            }
        }
        else if (manager.MustChangePassword)
        {
            manager.MarkPasswordAsChanged();
            var updateResult = await _userManager.UpdateAsync(manager);
            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(BuildIdentityErrorMessage(updateResult));
            }
        }

        if (!await _userManager.IsInRoleAsync(manager, IdentityRoleNames.Manager))
        {
            var roleResult = await _userManager.AddToRoleAsync(manager, IdentityRoleNames.Manager);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(BuildIdentityErrorMessage(roleResult));
            }
        }
    }

    private static string BuildIdentityErrorMessage(IdentityResult result)
    {
        return string.Join("; ", result.Errors.Select(error => error.Description));
    }
}
