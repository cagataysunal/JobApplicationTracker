namespace JobTracker.Api.Domain;

public enum InterviewType
{
    Screening,
    Technical,
    TakeHome,
    Behavioral,
    Final
}

public sealed class Interview : Entity
{
    public Guid JobApplicationId { get; set; }
    public InterviewType Type { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public string? Notes { get; set; }
}
