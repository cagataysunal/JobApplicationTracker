using Microsoft.Data.SqlClient;

namespace JobTracker.Api.Infrastructure.Persistence;

public static class ConnectionStringResolver
{
    public const string PasswordKey = "MSSQL_SA_PASSWORD";

    /// <summary>
    /// Returns the "Default" connection string. The tracked appsettings file holds
    /// no password; it is filled in from <see cref="PasswordKey"/> (.env, environment
    /// variable or user-secrets) so the secret never lands in source control.
    /// </summary>
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
