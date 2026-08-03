using Microsoft.EntityFrameworkCore;
using PTickets.Api.Common;
using PTickets.Api.Inspections.Data;
using PTickets.Api.Inspectors.Data;
using PTickets.Api.Penalties.Data;

namespace PTickets.Api.Database;

public class ParkingDbContext : DbContext
{

    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<Penalty> Penalties => Set<Penalty>();
    public DbSet<Inspector> Inspectors => Set<Inspector>();
    public DbSet<Zone> Zones => Set<Zone>();
    public ParkingDbContext(DbContextOptions<ParkingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- MAPOWANIE: Inspection ---
        modelBuilder.Entity<Inspection>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasConversion(id => id.Value, value => new InspectionId(value));

            builder.Property(x => x.ConductedBy)
                .HasConversion(id => id.Value, value => new InspectorId(value));

            builder.Property(x => x.ZoneId)
                .HasConversion(id => id.Value, value => new ZoneId(value));

            builder.Property(x => x.StreetId)
                .HasConversion(
                    id => id.HasValue ? id.Value.Value : (Guid?)null,
                    value => value.HasValue ? new StreetId(value.Value) : null);

            // Mapowanie Value Object: RegistrationNumber -> string
            builder.Property(x => x.RegistrationNumber)
                .HasConversion(reg => reg.Value, value => new RegistrationNumber(value));

            // Mapowanie Value Object: TicketCheckResult jako Owned Entity (zagnieżdżone kolumny)
            builder.OwnsOne(x => x.Result, resultBuilder =>
            {
                resultBuilder.Property(r => r.IsValid).HasColumnName("Result_IsValid");
                resultBuilder.Property(r => r.ValidFrom).HasColumnName("Result_ValidFrom");
                resultBuilder.Property(r => r.ValidTo).HasColumnName("Result_ValidTo");
                resultBuilder.Property(r => r.TicketProviderMessage).HasColumnName("Result_Message");
            });
        });

        // --- MAPOWANIE: Penalty ---
        modelBuilder.Entity<Penalty>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasConversion(id => id.Value, value => new PenaltyId(value));

            builder.Property(x => x.InspectionId)
                .HasConversion(id => id.Value, value => new InspectionId(value));

            builder.Property(x => x.RegistrationNumber)
                .HasConversion(reg => reg.Value, value => new RegistrationNumber(value));
        });

        // --- MAPOWANIE: Inspector ---
        modelBuilder.Entity<Inspector>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasConversion(id => id.Value, value => new InspectorId(value));

            builder.Property(x => x.AssignedToZone)
                .HasConversion(
                    id => id.HasValue ? id.Value.Value : (Guid?)null,
                    value => value.HasValue ? new ZoneId(value.Value) : null);
        });

        // --- MAPOWANIE: Zone & Street ---
        modelBuilder.Entity<Zone>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasConversion(id => id.Value, value => new ZoneId(value));

            // Mapowanie PaidParkingSchedule jako Owned Value Object
            builder.OwnsOne(x => x.PaidParkingSchedule, scheduleBuilder =>
            {
                scheduleBuilder.Property(s => s.StartTime).HasColumnName("PaidParkingSchedule_StartTime");
                scheduleBuilder.Property(s => s.EndTime).HasColumnName("PaidParkingSchedule_EndTime");
                scheduleBuilder.Property(s => s.PaidDays).HasColumnName("PaidParkingSchedule_PaidDays")
                    .HasConversion(
                        days => string.Join(",", days.Cast<int>()),
                        value => value.Split(",").Select(s => (DayOfWeek)int.Parse(s)).ToArray()
                    );
            });

            // Mapowanie ulic jako kolekcji wewnątrz Agregatu Strefy (Owned Collection)
            builder.OwnsMany(x => x.Streets, streetBuilder =>
            {
                streetBuilder.WithOwner().HasForeignKey("ZoneId");
                streetBuilder.HasKey(s => s.Id);
                streetBuilder.Property(s => s.Id)
                    .HasConversion(id => id.Value, value => new StreetId(value));
            });

            // Pozwala EF Core na dostęp do prywatnego pola _streets
            builder.Navigation(x => x.Streets).Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}
