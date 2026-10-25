using JobTracker.Api.Domain;

namespace JobTracker.Tests.Domain;

public class JobApplicationTests
{
    private static JobApplication NewApplication() => new() { Position = "Backend Engineer" };

    [Fact]
    public void New_application_starts_as_saved_without_applied_date()
    {
        var application = NewApplication();

        Assert.Equal(ApplicationStatus.Saved, application.Status);
        Assert.Null(application.AppliedOn);
    }

    [Fact]
    public void Moving_past_saved_without_a_date_stamps_today()
    {
        var application = NewApplication();

        application.ChangeStatus(ApplicationStatus.Applied);

        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), application.AppliedOn);
    }

    [Fact]
    public void Given_applied_date_is_used()
    {
        var application = NewApplication();
        var date = new DateOnly(2026, 9, 1);

        application.ChangeStatus(ApplicationStatus.Applied, date);

        Assert.Equal(date, application.AppliedOn);
    }

    [Fact]
    public void Later_status_change_keeps_existing_applied_date()
    {
        var application = NewApplication();
        var date = new DateOnly(2026, 9, 1);
        application.ChangeStatus(ApplicationStatus.Applied, date);

        application.ChangeStatus(ApplicationStatus.Interview);

        Assert.Equal(ApplicationStatus.Interview, application.Status);
        Assert.Equal(date, application.AppliedOn);
    }

    [Fact]
    public void Staying_saved_does_not_stamp_a_date()
    {
        var application = NewApplication();

        application.ChangeStatus(ApplicationStatus.Saved);

        Assert.Null(application.AppliedOn);
    }

    [Fact]
    public void Position_and_company_name_are_trimmed()
    {
        var application = new JobApplication { Position = "  Backend Engineer  " };
        var company = new Company { Name = "  Acme " };

        Assert.Equal("Backend Engineer", application.Position);
        Assert.Equal("Acme", company.Name);
    }
}
