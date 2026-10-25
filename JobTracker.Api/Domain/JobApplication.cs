namespace JobTracker.Api.Domain;

public sealed class JobApplication : Entity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public required string Position
    {
        get;
        set => field = value.Trim();
    }

    public string? PostingUrl { get; set; }
    public string? Location { get; set; }
    public WorkMode WorkMode { get; set; }
    public Salary? Salary { get; set; }
    public string? Notes { get; set; }

    public ApplicationStatus Status { get; private set; } = ApplicationStatus.Saved;
    public DateOnly? AppliedOn { get; private set; }

    public void ChangeStatus(ApplicationStatus status, DateOnly? appliedOn = null)
    {
        Status = status;
        AppliedOn = appliedOn ?? AppliedOn;

        if (status != ApplicationStatus.Saved && AppliedOn is null)
            AppliedOn = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
