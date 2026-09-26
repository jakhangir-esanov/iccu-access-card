namespace Iccu.Infrastructure.Database;

using Iccu.Domain.Users;
using Iccu.Domain.Readers;
using Iccu.Domain.StoredFiles;
using Iccu.Domain.RefreshTokens;
using Iccu.Infrastructure.Users;
using Iccu.Infrastructure.Readers;
using Microsoft.EntityFrameworkCore;
using Iccu.Infrastructure.StoredFiles;
using Iccu.Domain.RegistrationRequests;
using Iccu.Infrastructure.RefreshTokens;
using Iccu.Application.Abstractions.Data;
using Iccu.Infrastructure.RegistrationRequests;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IUnitOfWork
{
    internal DbSet<Reader> Readers { get; set; }

    internal DbSet<RegistrationRequest> RegistrationRequests { get; set; }

    internal DbSet<User> Users { get; set; }

    internal DbSet<RefreshToken> RefreshTokens { get; set; }

    internal DbSet<StoredFile> StoredFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Iccu);
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.HasSequence<int>(Sequences.CardNumber)
            .StartsAt(1)
            .HasMin(1)
            .HasMax(9_999_999)
            .IncrementsBy(1);

        modelBuilder.HasSequence<int>(Sequences.RegistrationCode)
            .StartsAt(1)
            .HasMin(1)
            .HasMax(9_999)
            .IncrementsBy(1)
            .IsCyclic();

        modelBuilder.ApplyConfiguration(new ReaderConfiguration());
        modelBuilder.ApplyConfiguration(new RegistrationRequestConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new StoredFileConfiguration());
    }
}
