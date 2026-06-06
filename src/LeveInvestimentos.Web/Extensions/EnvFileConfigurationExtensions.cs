using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace LeveInvestimentos.Web.Extensions;

public static class EnvFileConfigurationExtensions
{
    public static IConfigurationBuilder AddEnvFile(
        this IConfigurationBuilder configuration,
        string fileName,
        string startPath)
    {
        var envPath = FindFile(fileName, startPath);

        if (envPath is null)
        {
            return configuration;
        }

        var values = new Dictionary<string, string?>();

        foreach (var line in File.ReadAllLines(envPath))
        {
            var trimmed = line.Trim();

            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = trimmed.IndexOf('=');

            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = trimmed[..separatorIndex].Trim().Replace("__", ":");
            var value = trimmed[(separatorIndex + 1)..].Trim().Trim('"');

            values[key] = value;
        }

        return configuration.AddInMemoryCollection(values);
    }

    private static string? FindFile(string fileName, string startPath)
    {
        var directory = new DirectoryInfo(startPath);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, fileName);

            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
