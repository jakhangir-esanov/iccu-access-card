namespace Iccu.Infrastructure.RegistrationRequests;

using Iccu.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Iccu.Domain.RegistrationRequests;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class RegistrationRequestConfiguration : IEntityTypeConfiguration<RegistrationRequest>
{
    public void Configure(EntityTypeBuilder<RegistrationRequest> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Ignore(x => x.Details);

        builder.Property(x => x.Code)
            .HasDefaultValueSql(Sequences.NextValue(Sequences.RegistrationCode))
            .ValueGeneratedOnAdd();

        builder.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.MiddleName)
            .HasMaxLength(100);

        builder.Property(x => x.Phone)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(x => x.DocumentNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("status = 0");
        builder.HasIndex(x => new { x.Status, x.SubmittedAt });
        builder.HasIndex(x => x.PhotoFileId);
    }
}
