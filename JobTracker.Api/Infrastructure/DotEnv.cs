namespace JobTracker.Api.Infrastructure;

/// <summary>
/// Reads a docker-compose style .env file into configuration, so local secrets
/// live in one file that both docker compose and the API use.
/// </summary>
public static class DotEnv
{
    public static Dictionary<string, string?> Parse(IEnumerable<string> lines)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            var separator = line.IndexOf('=');
            if (separator <= 0)
                continue;

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim().Trim('"', '\'');
            values[key] = value;
        }

        return values;
    }

    extension(IConfigurationManager configuration)
    {
        /// <summary>
        /// Adds values from <paramref name="path"/> without overriding keys that are
        /// already set, so real environment variables and user-secrets still win.
        /// </summary>
        public void AddDotEnvFile(string path)
        {
            if (!File.Exists(path))
                return;

            var values = Parse(File.ReadLines(path))
                .Where(pair => configuration[pair.Key] is null)
                .ToDictionary();

            configuration.AddInMemoryCollection(values);
        }
    }
}
