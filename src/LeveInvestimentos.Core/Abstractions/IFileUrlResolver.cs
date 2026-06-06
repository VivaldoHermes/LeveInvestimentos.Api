namespace LeveInvestimentos.Core.Abstractions;

public interface IFileUrlResolver
{
    string ResolvePublicUrl(string storageKey);
}
