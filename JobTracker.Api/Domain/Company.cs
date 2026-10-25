namespace JobTracker.Api.Domain;

public sealed class Company : Entity
{
    public required string Name
    {
        get;
        // C# 14 `field` keyword: normalize without a hand-written backing field.
        set => field = value.Trim();
    }

    public string? Website { get; set; }
    public string? Country { get; set; }

    public List<JobApplication> Applications { get; } = [];
}
