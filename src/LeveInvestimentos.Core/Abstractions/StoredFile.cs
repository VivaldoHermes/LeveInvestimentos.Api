namespace LeveInvestimentos.Core.Abstractions;

public sealed record StoredFile(
    string StorageKey,
    string ContentType,
    long SizeBytes);
