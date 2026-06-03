namespace LeveInvestimentos.Core.Abstractions;

public sealed record StoredFile(
    string PublicUrl,
    string StorageKey,
    string ContentType,
    long SizeBytes);
