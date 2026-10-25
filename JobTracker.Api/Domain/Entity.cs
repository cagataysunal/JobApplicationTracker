namespace JobTracker.Api.Domain;

/// <summary>Base type for all aggregate roots and entities.</summary>
public abstract class Entity
{
    // Assigned by EF Core when the entity is added. On SQL Server, EF uses a
    // sequential GUID generator that matches how SQL Server sorts uniqueidentifier,
    // so inserts stay at the end of the clustered index.
    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; internal set; }
    public DateTimeOffset UpdatedAt { get; internal set; }
}
