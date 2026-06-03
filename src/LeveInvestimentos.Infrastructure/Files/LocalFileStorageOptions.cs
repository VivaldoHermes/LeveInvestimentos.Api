namespace LeveInvestimentos.Infrastructure.Files;

public sealed class LocalFileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string UploadRootPath { get; set; } = "wwwroot/uploads";

    public string PublicPathPrefix { get; set; } = "/uploads";
}
