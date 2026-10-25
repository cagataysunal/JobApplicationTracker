using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Domain;
using JobTracker.Api.Features.Applications;
using JobTracker.Api.Features.Companies;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Tests.Api;

public class JobApplicationEndpointsTests(ApiFactory factory) : ApiTestBase(factory)
{
    private static JobApplicationRequest Request(
        string companyName,
        ApplicationStatus status = ApplicationStatus.Saved,
        WorkMode workMode = WorkMode.Remote,
        SalaryDto? salary = null) =>
        new(companyName, "Senior .NET Engineer", null, null, workMode, salary, status, null, null);

    private async Task<JobApplicationResponse> CreateApplicationAsync(JobApplicationRequest request)
    {
        var response = await Client.PostAsJsonAsync("/api/applications", request, Json);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JobApplicationResponse>(Json))!;
    }

    [Fact]
    public async Task Create_returns_application_with_company_and_normalized_salary()
    {
        var company = await CreateCompanyAsync();
        var salary = new SalaryDto(80_000, 95_000, "eur", SalaryPeriod.Yearly);

        var application = await CreateApplicationAsync(Request(company.Name, ApplicationStatus.Applied, salary: salary));

        Assert.Equal(company.Id, application.CompanyId);
        Assert.Equal("EUR", application.Salary!.Currency);
        Assert.NotNull(application.AppliedOn);
    }

    [Fact]
    public async Task Create_without_salary_is_allowed()
    {
        var application = await CreateApplicationAsync(Request(Unique("Company")));

        Assert.Null(application.Salary);
        Assert.Null(application.AppliedOn);
    }

    [Fact]
    public async Task Create_with_new_company_name_creates_the_company()
    {
        var name = Unique("NewCo");

        var application = await CreateApplicationAsync(Request($"  {name}  "));

        var company = await Client.GetFromJsonAsync<CompanyResponse>($"/api/companies/{application.CompanyId}", Json);
        Assert.Equal(name, company!.Name);
        Assert.Equal(name, application.CompanyName);
    }

    [Fact]
    public async Task Company_name_match_is_case_insensitive_and_reuses_the_company()
    {
        var name = Unique("Acme");

        var first = await CreateApplicationAsync(Request(name));
        var second = await CreateApplicationAsync(Request(name.ToUpperInvariant()));

        Assert.Equal(first.CompanyId, second.CompanyId);
        Assert.Equal(name, second.CompanyName);
    }

    [Fact]
    public async Task Create_without_company_name_returns_validation_problem()
    {
        var response = await Client.PostAsJsonAsync("/api/applications", Request(""), Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json);
        Assert.Contains("CompanyName", problem!.Errors.Keys);
    }

    [Fact]
    public async Task Create_with_salary_min_above_max_returns_400()
    {
        var salary = new SalaryDto(100_000, 50_000, "USD", SalaryPeriod.Yearly);

        var response = await Client.PostAsJsonAsync(
            "/api/applications", Request(Unique("Company"), salary: salary), Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_filters_by_status_and_work_mode()
    {
        var company = Unique("Company");
        var match = await CreateApplicationAsync(Request(company, ApplicationStatus.Interview, WorkMode.Remote));
        var otherStatus = await CreateApplicationAsync(Request(company, ApplicationStatus.Applied, WorkMode.Remote));
        var otherMode = await CreateApplicationAsync(Request(company, ApplicationStatus.Interview, WorkMode.OnSite));

        var items = await Client.GetFromJsonAsync<List<JobApplicationResponse>>(
            "/api/applications?status=Interview&workMode=Remote", Json);

        var ids = items!.Select(a => a.Id).ToList();
        Assert.Contains(match.Id, ids);
        Assert.DoesNotContain(otherStatus.Id, ids);
        Assert.DoesNotContain(otherMode.Id, ids);
    }

    [Fact]
    public async Task Update_can_move_application_to_another_company()
    {
        var application = await CreateApplicationAsync(Request(Unique("First")));
        var secondName = Unique("Second");

        var response = await Client.PutAsJsonAsync(
            $"/api/applications/{application.Id}", Request(secondName, ApplicationStatus.Offer), Json);

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<JobApplicationResponse>(Json);
        Assert.Equal(secondName, updated!.CompanyName);
        Assert.NotEqual(application.CompanyId, updated.CompanyId);
        Assert.Equal(ApplicationStatus.Offer, updated.Status);
    }

    [Fact]
    public async Task Company_with_applications_cannot_be_deleted()
    {
        var application = await CreateApplicationAsync(Request(Unique("Company")));

        var response = await Client.DeleteAsync($"/api/companies/{application.CompanyId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_removes_application_then_returns_404()
    {
        var application = await CreateApplicationAsync(Request(Unique("Company")));

        var first = await Client.DeleteAsync($"/api/applications/{application.Id}");
        var second = await Client.DeleteAsync($"/api/applications/{application.Id}");

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
    }
}
