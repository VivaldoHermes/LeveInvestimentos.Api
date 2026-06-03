namespace LeveInvestimentos.Infrastructure.Files;

public sealed record ValidatedImageFile(
    byte[] Bytes,
    string Extension,
    string ContentType,
    long SizeBytes);
