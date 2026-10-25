using JobTracker.Api.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace JobTracker.Tests.Infrastructure;

public class ConnectionStringResolverTests
{
    private const string WithoutPassword = "Server=localhost,1433;Database=JobTracker;User Id=sa";

    private static IConfiguration Config(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(v => v.Key, v => (string?)v.Value))
            .Build();

    [Fact]
    public void Fills_password_from_configuration()
    {
        var config = Config(("ConnectionStrings:Default", WithoutPassword), ("MSSQL_SA_PASSWORD", "S3cret!"));

        var result = new SqlConnectionStringBuilder(ConnectionStringResolver.Resolve(config));

        Assert.Equal("S3cret!", result.Password);
        Assert.Equal("JobTracker", result.InitialCatalog);
    }

    [Fact]
    public void Keeps_password_already_in_connection_string()
    {
        var config = Config(
            ("ConnectionStrings:Default", WithoutPassword + ";Password=Explicit1!"),
            ("MSSQL_SA_PASSWORD", "Other1!"));

        var result = new SqlConnectionStringBuilder(ConnectionStringResolver.Resolve(config));

        Assert.Equal("Explicit1!", result.Password);
    }

    [Fact]
    public void Throws_when_connection_string_is_missing()
    {
        Assert.Throws<InvalidOperationException>(() => ConnectionStringResolver.Resolve(Config()));
    }
}
