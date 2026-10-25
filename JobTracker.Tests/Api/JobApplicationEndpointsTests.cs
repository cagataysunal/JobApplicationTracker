using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Domain;
using JobTracker.Api.Features.Applications;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Tests.Api;

public class JobApplicationEndpointsTests(ApiFactory factory) : ApiTestBase(factory)
{
    private static JobApplicationRequest Request(
        Guid companyId,
        ApplicationStatus status = ApplicationStatus.Saved,
        WorkMode workMode = WorkMode.Remote,
        SalaryDto? salary = null) =>
        new(companyId, "Senior .NET Engineer", null, null, workMode, salary, status, null, null);

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

        var application = await CreateApplicationAsync(Request(company.Id, ApplicationStatus.Applied, salary: salary));

        Assert.Equal(company.Name, application.CompanyName);
        Assert.Equal("EUR", application.Salary!.Currency);
        Assert.NotNull(application.AppliedOn);
    }

    [Fact]
    public async Task Create_without_salary_is_allowed()
    {
        var company = await CreateCompanyAsync();

        var application = await CreateApplicationAsync(Request(company.Id));

        Assert.Null(application.Salary);
        Assert.Null(application.AppliedOn);
    }

    [Fact]
    public async Task Create_for_unknown_company_returns_validation_problem()
    {
        var response = await Client.PostAsJsonAsync("/api/applications", Request(Guid.CreateVersion7()), Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json);
        Assert.Contains("CompanyId", problem!.Errors.Keys);
    }

    [Fact]
    public async Task Create_with_salary_min_above_max_returns_400()
    {
        var company = await CreateCompanyAsync();
        var salary = new SalaryDto(100_000, 50_000, "USD", SalaryPeriod.Yearly);

        var response = await Client.PostAsJsonAsync(
            "/api/applications", Request(company.Id, salary: salary), Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_filters_by_status_and_work_mode()
    {
        var company = await CreateCompanyAsync();
        var match = await CreateApplicationAsync(Request(company.Id, ApplicationStatus.Interview, WorkMode.Remote));
        var otherStatus = await CreateApplicationAsync(Request(company.Id, ApplicationStatus.Applied, WorkMode.Remote));
        var otherMode = await CreateApplicationAsync(Request(company.Id, ApplicationStatus.Interview, WorkMode.OnSite));

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
        var first = await CreateCompanyAsync();
        var second = await CreateCompanyAsync();
        var application = await CreateApplicationAsync(Request(first.Id));

        var response = await Client.PutAsJsonAsync(
            $"/api/applications/{application.Id}", Request(second.Id, ApplicationStatus.Offer), Json);

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<JobApplicationResponse>(Json);
        Assert.Equal(second.Name, updated!.CompanyName);
        Assert.Equal(ApplicationStatus.Offer, updated.Status);
    }

    [Fact]
    public async Task Company_with_applications_cannot_be_deleted()
    {
        var company = await CreateCompanyAsync();
        await CreateApplicationAsync(Request(company.Id));

        var response = await Client.DeleteAsync($"/api/companies/{company.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_removes_application_then_returns_404()
    {
        var company = await CreateCompanyAsync();
        var application = await CreateApplicationAsync(Request(company.Id));

        var first = await Client.DeleteAsync($"/api/applications/{application.Id}");
        var second = await Client.DeleteAsync($"/api/applications/{application.Id}");

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
    }
}
