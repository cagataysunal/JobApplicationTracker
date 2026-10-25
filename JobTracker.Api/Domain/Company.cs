namespace JobTracker.Api.Domain;

public sealed class Company : Entity
{
    public required string Name
    {
        get;
        set => field = value.Trim();
    }

    public string? Website { get; set; }
    public string? Country { get; set; }

    public List<JobApplication> Applications { get; } = [];
}
