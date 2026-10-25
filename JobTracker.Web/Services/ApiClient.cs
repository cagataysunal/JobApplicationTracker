using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using JobTracker.Web.Models;

namespace JobTracker.Web.Services;

public readonly record struct ApiResult(string? Error)
{
    public bool Ok => Error is null;
}

public sealed class ApiClient(HttpClient http)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<List<Company>> GetCompaniesAsync() =>
        await http.GetFromJsonAsync<List<Company>>("api/companies", Json) ?? [];

    public Task<ApiResult> CreateCompanyAsync(CompanyRequest request) =>
        SendAsync(http.PostAsJsonAsync("api/companies", request, Json));

    public Task<ApiResult> DeleteCompanyAsync(Guid id) =>
        SendAsync(http.DeleteAsync($"api/companies/{id}"));

    public async Task<List<JobApplication>> GetApplicationsAsync(ApplicationStatus? status, WorkMode? workMode)
    {
        var query = new List<string>();
        if (status is not null) query.Add($"status={status}");
        if (workMode is not null) query.Add($"workMode={workMode}");

        var url = query.Count == 0 ? "api/applications" : $"api/applications?{string.Join('&', query)}";
        return await http.GetFromJsonAsync<List<JobApplication>>(url, Json) ?? [];
    }

    public Task<ApiResult> CreateApplicationAsync(JobApplicationRequest request) =>
        SendAsync(http.PostAsJsonAsync("api/applications", request, Json));

    public Task<ApiResult> UpdateApplicationAsync(Guid id, JobApplicationRequest request) =>
        SendAsync(http.PutAsJsonAsync($"api/applications/{id}", request, Json));

    public Task<ApiResult> DeleteApplicationAsync(Guid id) =>
        SendAsync(http.DeleteAsync($"api/applications/{id}"));

    private static async Task<ApiResult> SendAsync(Task<HttpResponseMessage> request)
    {
        try
        {
            using var response = await request;
            return response.IsSuccessStatusCode
                ? new ApiResult(null)
                : new ApiResult(await ReadErrorAsync(response));
        }
        catch (HttpRequestException)
        {
            return new ApiResult("Could not reach the API. Is it running?");
        }
    }

    // The API returns RFC 9457 ProblemDetails for validation errors and a plain
    // JSON string for conflicts. Turn either into one readable line.
    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.String)
                return root.GetString()!;

            if (root.TryGetProperty("errors", out var errors))
                return string.Join(" ", errors.EnumerateObject()
                    .SelectMany(e => e.Value.EnumerateArray().Select(m => m.GetString())));

            if (root.TryGetProperty("title", out var title))
                return title.GetString()!;
        }
        catch (JsonException)
        {
        }

        return response.StatusCode == HttpStatusCode.NotFound ? "Not found." : $"Request failed ({(int)response.StatusCode}).";
    }
}
