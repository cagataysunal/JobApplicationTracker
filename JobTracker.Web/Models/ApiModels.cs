using System.ComponentModel.DataAnnotations;

namespace JobTracker.Web.Models;

// The client keeps its own copy of the API contract on purpose: the UI only
// depends on the HTTP shape, never on server-side domain or EF types.

public enum ApplicationStatus { Saved, Applied, ReceivedResponse, Interview, Rejected, Offer, Withdrawn, Ghosted }
public enum WorkMode { Remote, Hybrid, OnSite }
public enum SalaryPeriod { Hourly, Monthly, Yearly }

public sealed record Salary(decimal Min, decimal Max, string Currency, SalaryPeriod Period);

public sealed record Company(Guid Id, string Name, string? Website, string? Country);

public sealed record CompanyRequest(string Name, string? Website, string? Country);

public sealed record JobApplication(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    string Position,
    string? PostingUrl,
    string? Location,
    WorkMode WorkMode,
    Salary? Salary,
    ApplicationStatus Status,
    DateOnly? AppliedOn,
    string? Notes,
    DateTimeOffset UpdatedAt);

public sealed record JobApplicationRequest(
    Guid CompanyId,
    string Position,
    string? PostingUrl,
    string? Location,
    WorkMode WorkMode,
    Salary? Salary,
    ApplicationStatus Status,
    DateOnly? AppliedOn,
    string? Notes)
{
    public static JobApplicationRequest From(JobApplication a) => new(
        a.CompanyId, a.Position, a.PostingUrl, a.Location, a.WorkMode, a.Salary, a.Status, a.AppliedOn, a.Notes);
}

/// <summary>Mutable model bound to the application form.</summary>
public sealed class JobApplicationForm
{
    [Required(ErrorMessage = "Pick a company.")]
    public Guid? CompanyId { get; set; }

    [Required, MaxLength(200)]
    public string Position { get; set; } = "";

    [Url, MaxLength(1000)]
    public string? PostingUrl { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public WorkMode WorkMode { get; set; } = WorkMode.Remote;
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Saved;
    public DateOnly? AppliedOn { get; set; }

    [Range(0, 100_000_000)]
    public decimal? SalaryMin { get; set; }

    [Range(0, 100_000_000)]
    public decimal? SalaryMax { get; set; }

    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "EUR";

    public SalaryPeriod SalaryPeriod { get; set; } = SalaryPeriod.Yearly;

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public JobApplicationRequest ToRequest()
    {
        // Salary is optional: send it only when a range was entered.
        Salary? salary = SalaryMin is { } min && SalaryMax is { } max
            ? new Salary(min, max, Currency, SalaryPeriod)
            : null;

        return new JobApplicationRequest(
            CompanyId!.Value, Position, Empty(PostingUrl), Empty(Location),
            WorkMode, salary, Status, AppliedOn, Empty(Notes));
    }

    public static JobApplicationForm From(JobApplication a) => new()
    {
        CompanyId = a.CompanyId,
        Position = a.Position,
        PostingUrl = a.PostingUrl,
        Location = a.Location,
        WorkMode = a.WorkMode,
        Status = a.Status,
        AppliedOn = a.AppliedOn,
        SalaryMin = a.Salary?.Min,
        SalaryMax = a.Salary?.Max,
        Currency = a.Salary?.Currency ?? "EUR",
        SalaryPeriod = a.Salary?.Period ?? SalaryPeriod.Yearly,
        Notes = a.Notes
    };

    private static string? Empty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}

public sealed class CompanyForm
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = "";

    [Url, MaxLength(500)]
    public string? Website { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }
}
