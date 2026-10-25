using JobTracker.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobTracker.Api.Infrastructure.Persistence.Configurations;

internal sealed class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.Property(i => i.Notes).HasMaxLength(4000);
        builder.HasIndex(i => i.ScheduledAt);
    }
}
