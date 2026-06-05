using System.Threading.Tasks;
using LeveInvestimentos.Infrastructure.Persistence.Seed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LeveInvestimentos.Web.Extensions;

public static class HostExtensions
{
    public static async Task SeedDatabaseAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
}
