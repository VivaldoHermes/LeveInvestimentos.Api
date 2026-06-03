namespace LeveInvestimentos.Infrastructure.Files;

public sealed class ImageFileValidationOptions
{
    public const string SectionName = "ImageFiles";

    public long MaxFileSizeBytes { get; set; } = 2 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } =
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp"
    };
}
