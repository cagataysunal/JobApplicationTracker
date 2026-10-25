namespace JobTracker.Api.Domain;

public abstract class Entity
{
    // Left to EF Core: its sequential GUIDs match SQL Server's uniqueidentifier
    // sort order, so inserts stay at the end of the clustered index.
    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; internal set; }
    public DateTimeOffset UpdatedAt { get; internal set; }
}
