using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LeveInvestimentos.Web.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Full detail is logged server-side only; the client never sees the
            // exception message or stack trace (security / LGPD).
            _logger.LogError(
                ex,
                "Unhandled exception while processing {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "text/html; charset=utf-8";

            await context.Response.WriteAsync(
                "<!DOCTYPE html><html lang=\"pt-BR\"><head><meta charset=\"utf-8\">" +
                "<title>Erro inesperado</title></head><body>" +
                "<h1>Ocorreu um erro inesperado</h1>" +
                "<p>Tente novamente mais tarde. Se o problema persistir, contate o suporte.</p>" +
                "</body></html>");
        }
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
