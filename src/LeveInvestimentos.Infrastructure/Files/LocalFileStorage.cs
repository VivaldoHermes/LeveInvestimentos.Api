using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using Microsoft.Extensions.Options;

namespace LeveInvestimentos.Infrastructure.Files;

public sealed class LocalFileStorage : IFileStorage
{
    private const int BufferSize = 81920;

    private readonly LocalFileStorageOptions _options;
    private readonly ImageFileValidationOptions _validationOptions;

    public LocalFileStorage(
        IOptions<LocalFileStorageOptions> options,
        IOptions<ImageFileValidationOptions> validationOptions)
    {
        _options = options.Value;
        _validationOptions = validationOptions.Value;
    }

    public async Task<StoredFile> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var validatedFile = await ValidateImageAsync(
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

    private async Task<ValidatedImageFile> ValidateImageAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var allowedExtensions = GetAllowedExtensions();

        if (!allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("File extension is not allowed.");
        }

        var bytes = await ReadContentAsync(content, _validationOptions.MaxFileSizeBytes, cancellationToken);

        if (!MatchesAllowedImageSignature(bytes, extension))
        {
            throw new InvalidOperationException("File content does not match an allowed image format.");
        }

        return new ValidatedImageFile(bytes, extension, GetContentType(extension), bytes.LongLength);
    }

    private HashSet<string> GetAllowedExtensions()
    {
        return _validationOptions.AllowedExtensions
            .Select(extension => extension.ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static async Task<byte[]> ReadContentAsync(
        Stream content,
        long maxFileSizeBytes,
        CancellationToken cancellationToken)
    {
        if (maxFileSizeBytes <= 0)
        {
            throw new InvalidOperationException("Maximum file size must be greater than zero.");
        }

        using var memoryStream = new MemoryStream();
        var buffer = new byte[BufferSize];
        long totalBytes = 0;

        while (true)
        {
            var bytesRead = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            totalBytes += bytesRead;

            if (totalBytes > maxFileSizeBytes)
            {
                throw new InvalidOperationException("File exceeds the maximum allowed size.");
            }

            await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        }

        if (memoryStream.Length == 0)
        {
            throw new InvalidOperationException("File content is empty.");
        }

        return memoryStream.ToArray();
    }

    private static bool MatchesAllowedImageSignature(byte[] bytes, string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" => IsJpeg(bytes),
            ".png" => IsPng(bytes),
            ".gif" => IsGif(bytes),
            ".webp" => IsWebp(bytes),
            _ => false
        };
    }

    private static string GetContentType(string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    private static bool IsJpeg(byte[] bytes)
    {
        return bytes.Length >= 3
            && bytes[0] == 0xFF
            && bytes[1] == 0xD8
            && bytes[2] == 0xFF;
    }

    private static bool IsPng(byte[] bytes)
    {
        return bytes.Length >= 8
            && bytes[0] == 0x89
            && bytes[1] == 0x50
            && bytes[2] == 0x4E
            && bytes[3] == 0x47
            && bytes[4] == 0x0D
            && bytes[5] == 0x0A
            && bytes[6] == 0x1A
            && bytes[7] == 0x0A;
    }

    private static bool IsGif(byte[] bytes)
    {
        return bytes.Length >= 6
            && bytes[0] == 0x47
            && bytes[1] == 0x49
            && bytes[2] == 0x46
            && bytes[3] == 0x38
            && (bytes[4] == 0x37 || bytes[4] == 0x39)
            && bytes[5] == 0x61;
    }

    private static bool IsWebp(byte[] bytes)
    {
        return bytes.Length >= 12
            && bytes[0] == 0x52
            && bytes[1] == 0x49
            && bytes[2] == 0x46
            && bytes[3] == 0x46
            && bytes[8] == 0x57
            && bytes[9] == 0x45
            && bytes[10] == 0x42
            && bytes[11] == 0x50;
    }

    private sealed record ValidatedImageFile(
        byte[] Bytes,
        string Extension,
        string ContentType,
        long SizeBytes);
}
