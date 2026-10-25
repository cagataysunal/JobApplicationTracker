using JobTracker.Api.Domain;
using JobTracker.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Features.Applications;

public static class JobApplicationEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapJobApplicationEndpoints()
        {
            var group = app.MapGroup("/api/applications").WithTags("Job Applications");

            group.MapGet("/", GetAll);
            group.MapGet("/{id:guid}", GetById).WithName(GetApplicationById);
            group.MapPost("/", Create);
            group.MapPut("/{id:guid}", Update);
            group.MapDelete("/{id:guid}", Delete);

            return app;
        }
    }

    private const string GetApplicationById = nameof(GetApplicationById);

    private static async Task<Ok<List<JobApplicationResponse>>> GetAll(
        ApplicationStatus? status, WorkMode? workMode, AppDbContext db, CancellationToken ct)
    {
        var query = db.JobApplications.AsNoTracking().Include(a => a.Company).AsQueryable();

        if (status is not null)
            query = query.Where(a => a.Status == status);
        if (workMode is not null)
            query = query.Where(a => a.WorkMode == workMode);

        var items = await query.OrderByDescending(a => a.UpdatedAt).ToListAsync(ct);
        return TypedResults.Ok(items.Select(JobApplicationResponse.From).ToList());
    }

    private static async Task<Results<Ok<JobApplicationResponse>, NotFound>> GetById(
        Guid id, AppDbContext db, CancellationToken ct) =>
        await db.JobApplications.AsNoTracking().Include(a => a.Company)
            .FirstOrDefaultAsync(a => a.Id == id, ct) is { } application
            ? TypedResults.Ok(JobApplicationResponse.From(application))
            : TypedResults.NotFound();

    private static async Task<Results<CreatedAtRoute<JobApplicationResponse>, ValidationProblem>> Create(
        JobApplicationRequest request, AppDbContext db, CancellationToken ct)
    {
        var company = await db.Companies.FindAsync([request.CompanyId], ct);
        if (company is null)
            return CompanyNotFound();

        var application = new JobApplication { Position = request.Position, Company = company };
        Apply(application, request);

        db.JobApplications.Add(application);
        await db.SaveChangesAsync(ct);

        return TypedResults.CreatedAtRoute(
            JobApplicationResponse.From(application), GetApplicationById, new { id = application.Id });
    }

    private static async Task<Results<Ok<JobApplicationResponse>, NotFound, ValidationProblem>> Update(
        Guid id, JobApplicationRequest request, AppDbContext db, CancellationToken ct)
    {
        var application = await db.JobApplications.Include(a => a.Company)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
        if (application is null)
            return TypedResults.NotFound();

        if (application.CompanyId != request.CompanyId)
        {
            var company = await db.Companies.FindAsync([request.CompanyId], ct);
            if (company is null)
                return CompanyNotFound();
            application.Company = company;
        }

        Apply(application, request);
        await db.SaveChangesAsync(ct);

        return TypedResults.Ok(JobApplicationResponse.From(application));
    }

    private static async Task<Results<NoContent, NotFound>> Delete(Guid id, AppDbContext db, CancellationToken ct) =>
        await db.JobApplications.Where(a => a.Id == id).ExecuteDeleteAsync(ct) > 0
            ? TypedResults.NoContent()
            : TypedResults.NotFound();

    private static void Apply(JobApplication application, JobApplicationRequest request)
    {
        application.Position = request.Position;
        application.PostingUrl = request.PostingUrl;
        application.Location = request.Location;
        application.WorkMode = request.WorkMode;
        application.Salary = request.Salary?.ToDomain();
        application.Notes = request.Notes;
        application.ChangeStatus(request.Status, request.AppliedOn);
    }

    private static ValidationProblem CompanyNotFound() =>
        TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(JobApplicationRequest.CompanyId)] = ["Company does not exist."]
        });
}
