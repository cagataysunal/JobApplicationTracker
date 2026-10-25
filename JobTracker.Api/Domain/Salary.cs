namespace JobTracker.Api.Domain;

public enum SalaryPeriod
{
    Hourly,
    Monthly,
    Yearly
}

/// <summary>
/// Salary range for a position. Stored as an EF Core complex type (value object),
/// so it lives in the JobApplications table without its own identity.
/// </summary>
public sealed record Salary(decimal Min, decimal Max, string Currency, SalaryPeriod Period);
