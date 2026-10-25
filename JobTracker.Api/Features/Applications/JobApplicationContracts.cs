using System.ComponentModel.DataAnnotations;
using JobTracker.Api.Domain;

namespace JobTracker.Api.Features.Applications;

public sealed record SalaryDto(
    [property: Range(0, 100_000_000)] decimal Min,
    [property: Range(0, 100_000_000)] decimal Max,
    [property: Required, Length(3, 3)] string Currency,
    SalaryPeriod Period) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Min > Max)
            yield return new ValidationResult("Min cannot be greater than Max.", [nameof(Min), nameof(Max)]);
    }

    public Salary ToDomain() => new(Min, Max, Currency.ToUpperInvariant(), Period);
    public static SalaryDto? From(Salary? s) => s is null ? null : new(s.Min, s.Max, s.Currency, s.Period);
}

public sealed record JobApplicationRequest(
    [property: Required, MaxLength(200)] string CompanyName,
    [property: Required, MaxLength(200)] string Position,
    [property: Url, MaxLength(1000)] string? PostingUrl,
    [property: MaxLength(200)] string? Location,
    WorkMode WorkMode,
    SalaryDto? Salary,
    ApplicationStatus Status,
    DateOnly? AppliedOn,
    [property: MaxLength(4000)] string? Notes);

public sealed record JobApplicationResponse(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    string Position,
    string? PostingUrl,
    string? Location,
    WorkMode WorkMode,
    SalaryDto? Salary,
    ApplicationStatus Status,
    DateOnly? AppliedOn,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static JobApplicationResponse From(JobApplication a) => new(
        a.Id, a.CompanyId, a.Company.Name, a.Position, a.PostingUrl, a.Location, a.WorkMode,
        SalaryDto.From(a.Salary), a.Status, a.AppliedOn, a.Notes, a.CreatedAt, a.UpdatedAt);
}
