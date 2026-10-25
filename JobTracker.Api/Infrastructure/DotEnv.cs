namespace JobTracker.Api.Infrastructure;

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
        // Keys that are already set are kept, so real environment variables and
        // user-secrets take precedence over .env.
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
