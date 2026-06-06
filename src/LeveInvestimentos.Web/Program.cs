using LeveInvestimentos.Core;
using LeveInvestimentos.Infrastructure;
using LeveInvestimentos.Web.Extensions;
using LeveInvestimentos.Web.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvFile(".env", builder.Environment.ContentRootPath);
builder.AddSerilogLogging();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddWebPresentation();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandling();
    app.UseHsts();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();

await app.SeedDatabaseAsync();

app.Run();
