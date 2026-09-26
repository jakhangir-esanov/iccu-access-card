namespace Iccu.Infrastructure.StoredFiles;

using Iccu.Domain.StoredFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.OriginalName)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(f => f.Extension)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(f => f.ContentType)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(f => f.StoragePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(f => f.CreatedAt);
    }
}
