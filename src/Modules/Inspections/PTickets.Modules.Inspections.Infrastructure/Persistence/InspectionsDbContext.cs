using Microsoft.EntityFrameworkCore;
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
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inspections");

        modelBuilder.Entity<Session>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasConversion(x => x.Value, x => new SessionId(x));
            b.Property(x => x.InspectorId).HasConversion(x => x.Value, x => new InspectorId(x));
            b.Property(x => x.SelectedZoneId).HasConversion(x => x.HasValue ? (Guid?)x.Value.Value : null, x => x.HasValue ? new ZoneId(x.Value) : null);
            b.Property(x => x.SelectedStreetId).HasConversion(x => x.HasValue ? (Guid?)x.Value.Value : null, x => x.HasValue ? new StreetId(x.Value) : null);
        });

        modelBuilder.Entity<Inspection>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasConversion(x => x.Value, x => new InspectionId(x));
            b.Property(x => x.SessionId).HasConversion(x => x.Value, x => new SessionId(x));
            b.Property(x => x.InspectorId).HasConversion(x => x.Value, x => new InspectorId(x));
            b.Property(x => x.ZoneId).HasConversion(x => x.Value, x => new ZoneId(x));
            b.Property(x => x.StreetId).HasConversion(x => x.Value, x => new StreetId(x));
            b.Property(x => x.RegistrationNumber).HasConversion(x => x.Value, x => new RegistrationNumber(x));
            b.Property(x => x.Status).HasConversion<string>();
            b.Property(x => x.NoticeId).HasConversion(x => x.HasValue ? (Guid?)x.Value.Value : null, x => x.HasValue ? new NoticeId(x.Value) : null);
            
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

            b.Property(x => x.PhotoIds)
                .HasConversion(
                    v => string.Join(',', v.Select(id => id.Value)),
                    v => string.IsNullOrEmpty(v) ? new List<FileId>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(id => new FileId(Guid.Parse(id))).ToList()
                )
                .HasColumnName("PhotoIds");

            b.HasMany(x => x.Violations).WithOne().HasForeignKey(x => x.InspectionId);
        });

        modelBuilder.Entity<ViolationEntry>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.InspectionId).HasConversion(x => x.Value, x => new InspectionId(x));
            b.Property(x => x.ViolationTypeId).HasConversion(x => x.Value, x => new ViolationTypeId(x));
            b.Property(x => x.Source).HasConversion<string>();
        });


    }
}

