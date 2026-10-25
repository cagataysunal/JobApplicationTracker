using System.ComponentModel.DataAnnotations;
using JobTracker.Api.Domain;

namespace JobTracker.Api.Features.Companies;

public sealed record CompanyRequest(
    [property: Required, MaxLength(200)] string Name,
    [property: Url, MaxLength(500)] string? Website,
    [property: MaxLength(100)] string? Country);

public sealed record CompanyResponse(
    Guid Id,
    string Name,
    string? Website,
    string? Country,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static CompanyResponse From(Company c) =>
        new(c.Id, c.Name, c.Website, c.Country, c.CreatedAt, c.UpdatedAt);
}
