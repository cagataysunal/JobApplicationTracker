namespace JobTracker.Api.Domain;

public enum SalaryPeriod
{
    Hourly,
    Monthly,
    Yearly
}

public sealed record Salary(decimal Min, decimal Max, string Currency, SalaryPeriod Period);
