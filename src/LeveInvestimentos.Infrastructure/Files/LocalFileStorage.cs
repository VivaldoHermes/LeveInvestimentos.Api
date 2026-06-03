using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using Microsoft.Extensions.Options;

namespace LeveInvestimentos.Infrastructure.Files;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly LocalFileStorageOptions _options;
    private readonly IImageFileValidator _imageFileValidator;

    public LocalFileStorage(
        IOptions<LocalFileStorageOptions> options,
        IImageFileValidator imageFileValidator)
    {
        _options = options.Value;
        _imageFileValidator = imageFileValidator;
    }

    public async Task<StoredFile> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var validatedFile = await _imageFileValidator.ValidateAsync(
            content,
            fileName,
            contentType,
            cancellationToken);

        Directory.CreateDirectory(_options.UploadRootPath);

        var storedFileName = $"{Guid.NewGuid():N}{validatedFile.Extension}";
        var destinationPath = Path.Combine(_options.UploadRootPath, storedFileName);

        await File.WriteAllBytesAsync(destinationPath, validatedFile.Bytes, cancellationToken);

        var publicUrl = BuildPublicPath(storedFileName);

        return new StoredFile(
            publicUrl,
            storedFileName,
            validatedFile.ContentType,
            validatedFile.SizeBytes);
    }

    private string BuildPublicPath(string storedFileName)
    {
        var publicPathPrefix = _options.PublicPathPrefix.Trim('/').Replace('\\', '/');

        if (string.IsNullOrWhiteSpace(publicPathPrefix))
        {
            return "/" + storedFileName;
        }

        return "/" + publicPathPrefix + "/" + storedFileName;
    }
}
