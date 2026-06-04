using System.IO;
using LeveInvestimentos.Core.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeveInvestimentos.Infrastructure.Files;

public static class FileStorageServiceCollectionExtensions
{
    public static IServiceCollection AddLocalFileStorage(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath)
    {
        services.Configure<LocalFileStorageOptions>(
            configuration.GetSection(LocalFileStorageOptions.SectionName));
        services.Configure<ImageFileValidationOptions>(
            configuration.GetSection(ImageFileValidationOptions.SectionName));

        services.PostConfigure<LocalFileStorageOptions>(options =>
            NormalizeUploadRootPath(options, contentRootPath));

        services.AddScoped<IFileStorage, LocalFileStorage>();

        return services;
    }

    private static void NormalizeUploadRootPath(
        LocalFileStorageOptions options,
        string contentRootPath)
    {
        if (string.IsNullOrWhiteSpace(options.UploadRootPath))
        {
            options.UploadRootPath = Path.Combine(contentRootPath, "wwwroot", "uploads");
            return;
        }

        if (!Path.IsPathRooted(options.UploadRootPath))
        {
            options.UploadRootPath = Path.Combine(contentRootPath, options.UploadRootPath);
        }
    }
}
