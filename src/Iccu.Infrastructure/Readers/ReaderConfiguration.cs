namespace Iccu.Infrastructure.Readers;

using Iccu.Domain.Readers;
using Iccu.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ReaderConfiguration : IEntityTypeConfiguration<Reader>
{
    private const string SearchText = "SearchText";

    public void Configure(EntityTypeBuilder<Reader> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CardNumber)
            .HasDefaultValueSql(Sequences.NextValue(Sequences.CardNumber))
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

        builder.Property<string>(SearchText)
            .HasComputedColumnSql(
                "translate(lower(last_name || ' ' || first_name || ' ' || coalesce(middle_name, '')), '‘’ʻʼ`´', '''''''''''''')",
                stored: true);

        builder.HasIndex(x => x.CardNumber).IsUnique();
        builder.HasIndex(x => new { x.DocumentType, x.DocumentNumber })
            .IsUnique()
            .HasFilter("deleted_at IS NULL");
        builder.HasIndex(SearchText).HasMethod("gin").HasOperators("gin_trgm_ops");
        builder.HasIndex(x => x.Phone).HasMethod("gin").HasOperators("gin_trgm_ops");
        builder.HasIndex(x => x.PhotoFileId);
        builder.HasIndex(x => x.ExpiresOn);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
