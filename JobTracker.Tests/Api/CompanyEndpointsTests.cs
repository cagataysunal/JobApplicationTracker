using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Features.Companies;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Tests.Api;

public class CompanyEndpointsTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Create_returns_201_with_location_and_trimmed_name()
    {
        var name = Unique("Acme");

        var response = await Client.PostAsJsonAsync(
            "/api/companies", new CompanyRequest($"  {name}  ", "https://acme.io", "Netherlands"), Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var company = await response.Content.ReadFromJsonAsync<CompanyResponse>(Json);
        Assert.Equal(name, company!.Name);
        Assert.EndsWith($"/api/companies/{company.Id}", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Create_with_duplicate_name_returns_409()
    {
        var existing = await CreateCompanyAsync();

        var response = await Client.PostAsJsonAsync(
            "/api/companies", new CompanyRequest(existing.Name, null, null), Json);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_with_invalid_body_returns_validation_problem()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/companies", new CompanyRequest("", "not-a-url", null), Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json);
        Assert.Contains("Name", problem!.Errors.Keys);
        Assert.Contains("Website", problem.Errors.Keys);
    }

    [Fact]
    public async Task Get_unknown_id_returns_404()
    {
        var response = await Client.GetAsync($"/api/companies/{Guid.CreateVersion7()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_changes_fields_and_bumps_updated_at()
    {
        var company = await CreateCompanyAsync();
        var newName = Unique("Renamed");

        var response = await Client.PutAsJsonAsync(
            $"/api/companies/{company.Id}", new CompanyRequest(newName, null, "Germany"), Json);

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<CompanyResponse>(Json);
        Assert.Equal(newName, updated!.Name);
        Assert.Equal("Germany", updated.Country);
        Assert.True(updated.UpdatedAt > company.UpdatedAt);
    }

    [Fact]
    public async Task Delete_removes_company()
    {
        var company = await CreateCompanyAsync();

        var delete = await Client.DeleteAsync($"/api/companies/{company.Id}");
        var get = await Client.GetAsync($"/api/companies/{company.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }
}
