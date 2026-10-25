namespace JobTracker.Api.Domain;

public enum ApplicationStatus
{
    Saved,
    Applied,
    ReceivedResponse,
    Interview,
    Rejected,
    Offer,
    Withdrawn,
    Ghosted
}
