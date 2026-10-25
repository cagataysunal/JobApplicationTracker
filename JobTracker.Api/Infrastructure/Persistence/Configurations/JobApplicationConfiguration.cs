using JobTracker.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobTracker.Api.Infrastructure.Persistence.Configurations;

internal sealed class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.Property(a => a.Position).HasMaxLength(200);
        builder.Property(a => a.PostingUrl).HasMaxLength(1000);
        builder.Property(a => a.Location).HasMaxLength(200);
        builder.Property(a => a.Notes).HasMaxLength(4000);

        builder.ComplexProperty(a => a.Salary, salary =>
        {
            salary.Property(s => s.Min).HasPrecision(12, 2);
            salary.Property(s => s.Max).HasPrecision(12, 2);
            salary.Property(s => s.Currency).HasMaxLength(3).IsFixedLength().IsUnicode(false);
        });

        builder.HasIndex(a => a.Status);
    }
}
