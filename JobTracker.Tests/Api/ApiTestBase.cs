using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using JobTracker.Api.Features.Companies;

namespace JobTracker.Tests.Api;

[Collection(nameof(ApiCollection))]
public abstract class ApiTestBase(ApiFactory factory)
{
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected HttpClient Client { get; } = factory.CreateClient();

    // Tests share one database, so every test uses unique names instead of cleaning up.
    protected static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    protected async Task<CompanyResponse> CreateCompanyAsync(string? name = null)
    {
        var response = await Client.PostAsJsonAsync(
            "/api/companies", new CompanyRequest(name ?? Unique("Company"), null, null), Json);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CompanyResponse>(Json))!;
    }
}
