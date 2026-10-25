using JobTracker.Api.Domain;
using JobTracker.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Features.Companies;

public static class CompanyEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapCompanyEndpoints()
        {
            var group = app.MapGroup("/api/companies").WithTags("Companies");

            group.MapGet("/", GetAll);
            group.MapGet("/{id:guid}", GetById).WithName(nameof(GetCompanyById));
            group.MapPost("/", Create);
            group.MapPut("/{id:guid}", Update);
            group.MapDelete("/{id:guid}", Delete);

            return app;
        }
    }

    private const string GetCompanyById = nameof(GetCompanyById);

    private static async Task<Ok<List<CompanyResponse>>> GetAll(AppDbContext db, CancellationToken ct)
    {
        var companies = await db.Companies
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CompanyResponse(c.Id, c.Name, c.Website, c.Country, c.CreatedAt, c.UpdatedAt))
            .ToListAsync(ct);

        return TypedResults.Ok(companies);
    }

    private static async Task<Results<Ok<CompanyResponse>, NotFound>> GetById(
        Guid id, AppDbContext db, CancellationToken ct) =>
        await db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct) is { } company
            ? TypedResults.Ok(CompanyResponse.From(company))
            : TypedResults.NotFound();

    private static async Task<Results<CreatedAtRoute<CompanyResponse>, Conflict<string>>> Create(
        CompanyRequest request, AppDbContext db, CancellationToken ct)
    {
        if (await db.Companies.AnyAsync(c => c.Name == request.Name.Trim(), ct))
            return TypedResults.Conflict($"Company '{request.Name}' already exists.");

        var company = new Company
        {
            Name = request.Name,
            Website = request.Website,
            Country = request.Country
        };

        db.Companies.Add(company);
        await db.SaveChangesAsync(ct);

        return TypedResults.CreatedAtRoute(CompanyResponse.From(company), GetCompanyById, new { id = company.Id });
    }

    private static async Task<Results<Ok<CompanyResponse>, NotFound, Conflict<string>>> Update(
        Guid id, CompanyRequest request, AppDbContext db, CancellationToken ct)
    {
        var company = await db.Companies.FindAsync([id], ct);
        if (company is null)
            return TypedResults.NotFound();

        if (await db.Companies.AnyAsync(c => c.Id != id && c.Name == request.Name.Trim(), ct))
            return TypedResults.Conflict($"Company '{request.Name}' already exists.");

        company.Name = request.Name;
        company.Website = request.Website;
        company.Country = request.Country;
        await db.SaveChangesAsync(ct);

        return TypedResults.Ok(CompanyResponse.From(company));
    }

    private static async Task<Results<NoContent, NotFound, Conflict<string>>> Delete(
        Guid id, AppDbContext db, CancellationToken ct)
    {
        var company = await db.Companies.FindAsync([id], ct);
        if (company is null)
            return TypedResults.NotFound();

        if (await db.JobApplications.AnyAsync(a => a.CompanyId == id, ct))
            return TypedResults.Conflict("Company has job applications. Delete them first.");

        db.Companies.Remove(company);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
