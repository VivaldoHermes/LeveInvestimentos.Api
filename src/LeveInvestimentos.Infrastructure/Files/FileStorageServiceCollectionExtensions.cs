using System.IO;
using LeveInvestimentos.Core.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LeveInvestimentos.Infrastructure.Files;

public static class FileStorageServiceCollectionExtensions
{
    public static IServiceCollection AddLocalFileStorage(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath)
    {
        services.Configure<ImageFileValidationOptions>(
            configuration.GetSection(ImageFileValidationOptions.SectionName));

        var uploadRootPath = NormalizeUploadRootPath(
            configuration["FileStorage:UploadRootPath"],
            contentRootPath);
        var publicPathPrefix = configuration["FileStorage:PublicPathPrefix"] ?? "/uploads";

        services.AddScoped<LocalFileStorage>(provider =>
            new LocalFileStorage(
                uploadRootPath,
                publicPathPrefix,
                provider.GetRequiredService<IOptions<ImageFileValidationOptions>>()));

        services.AddScoped<IFileStorage>(provider => provider.GetRequiredService<LocalFileStorage>());
        services.AddScoped<IFileUrlResolver>(provider => provider.GetRequiredService<LocalFileStorage>());

        return services;
    }

    private static string NormalizeUploadRootPath(
        string? uploadRootPath,
        string contentRootPath)
    {
        if (string.IsNullOrWhiteSpace(uploadRootPath))
        {
            return Path.Combine(contentRootPath, "wwwroot", "uploads");
        }

        if (!Path.IsPathRooted(uploadRootPath))
        {
            return Path.Combine(contentRootPath, uploadRootPath);
        }

        return uploadRootPath;
    }
}
