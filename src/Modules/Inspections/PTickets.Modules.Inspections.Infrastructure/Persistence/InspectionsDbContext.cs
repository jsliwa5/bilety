using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

namespace PTickets.Modules.Inspections.Infrastructure.Persistence;

public class InspectionsDbContext : DbContext
{
    public InspectionsDbContext(DbContextOptions<InspectionsDbContext> options) : base(options)
    {
    }

    public DbSet<Inspection> Inspections { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;
    public DbSet<ViolationEntry> ViolationEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inspections");

        modelBuilder.Entity<Session>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Id).HasConversion(x => x.Value, x => new SessionId(x));
            b.Property(x => x.InspectorId).HasConversion(x => x.Value, x => new InspectorId(x));
        });

        modelBuilder.Entity<Inspection>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Id).HasConversion(x => x.Value, x => new InspectionId(x));
            b.Property(x => x.SessionId).HasConversion(x => x.Value, x => new SessionId(x));
            b.Property(x => x.InspectorId).HasConversion(x => x.Value, x => new InspectorId(x));
            b.Property(x => x.ZoneId).HasConversion(x => x.Value, x => new ZoneId(x));
            b.Property(x => x.StreetId).HasConversion(x => x.Value, x => new StreetId(x));
            b.Property(x => x.RegistrationNumber).HasConversion(x => x.Value, x => new RegistrationNumber(x));
            b.Property(x => x.Status).HasConversion<string>();
            b.Property(x => x.NoticeId).HasConversion(
                x => x.HasValue ? (Guid?)x.Value.Value : null,
                x => x.HasValue ? new NoticeId(x.Value) : null
            );

            b.OwnsOne(x => x.TicketResult, t =>
            {
                t.Property(x => x.IsValid).HasColumnName("TicketIsValid");
                t.Property(x => x.ValidFrom).HasColumnName("TicketValidFrom");
                t.Property(x => x.ValidTo).HasColumnName("TicketValidTo");
                t.Property(x => x.ProviderMessage).HasColumnName("TicketProviderMessage");
            });

            b.OwnsOne(x => x.SecondCheckResult, t =>
            {
                t.Property(x => x.IsValid).HasColumnName("SecondCheckIsValid");
                t.Property(x => x.ValidFrom).HasColumnName("SecondCheckValidFrom");
                t.Property(x => x.ValidTo).HasColumnName("SecondCheckValidTo");
                t.Property(x => x.ProviderMessage).HasColumnName("SecondCheckProviderMessage");
            });

            var photoIdsComparer = new ValueComparer<IReadOnlyCollection<FileId>>(
    (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
    c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
    c => c == null ? new List<FileId>() : c.ToList()
);

            b.Property(x => x.PhotoIds)
                .HasConversion(
                    v => v != null ? string.Join(',', v.Select(id => id.Value)) : string.Empty,
                    v => string.IsNullOrEmpty(v)
                        ? new List<FileId>()
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(id => new FileId(Guid.Parse(id)))
                            .ToList()
                )
                .HasColumnName("PhotoIds")
                .Metadata.SetValueComparer(photoIdsComparer);

            b.HasMany(x => x.Violations)
                .WithOne()
                .HasForeignKey(x => x.InspectionId);
        });

        modelBuilder.Entity<ViolationEntry>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.InspectionId).HasConversion(x => x.Value, x => new InspectionId(x));
            b.Property(x => x.ViolationTypeId).HasConversion(x => x.Value, x => new ViolationTypeId(x));
            b.Property(x => x.Source).HasConversion<string>();
        });
    }
}