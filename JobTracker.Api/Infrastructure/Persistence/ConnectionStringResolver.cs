using Microsoft.Data.SqlClient;

namespace JobTracker.Api.Infrastructure.Persistence;

public static class ConnectionStringResolver
{
    public const string PasswordKey = "MSSQL_SA_PASSWORD";

    // The tracked appsettings file has no password, so it never lands in source control.
    public static string Resolve(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        var builder = new SqlConnectionStringBuilder(connectionString);

        if (string.IsNullOrEmpty(builder.Password) && configuration[PasswordKey] is { Length: > 0 } password)
            builder.Password = password;

        return builder.ConnectionString;
    }
}
