using JobTracker.Api.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace JobTracker.Tests.Infrastructure;

public class DotEnvTests
{
    [Fact]
    public void Parse_reads_pairs_and_skips_comments_and_blank_lines()
    {
        var values = DotEnv.Parse(
        [
            "# comment",
            "",
            "MSSQL_SA_PASSWORD=Abc=123!",
            "  QUOTED = \"value with spaces\"  ",
            "not a pair"
        ]);

        Assert.Equal(2, values.Count);
        Assert.Equal("Abc=123!", values["MSSQL_SA_PASSWORD"]);
        Assert.Equal("value with spaces", values["QUOTED"]);
    }

    [Fact]
    public void AddDotEnvFile_does_not_override_existing_values()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(path, ["FROM_FILE=file", "ALREADY_SET=file"]);
            var configuration = new ConfigurationManager();
            configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["ALREADY_SET"] = "env" });

            configuration.AddDotEnvFile(path);

            Assert.Equal("file", configuration["FROM_FILE"]);
            Assert.Equal("env", configuration["ALREADY_SET"]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void AddDotEnvFile_ignores_missing_file()
    {
        var configuration = new ConfigurationManager();

        configuration.AddDotEnvFile("/does/not/exist/.env");

        Assert.Empty(configuration.AsEnumerable());
    }
}
