namespace PTickets.Modules.Zones.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PTickets.Modules.Zones.Domain;
using PTickets.Shared;

public class ZonesDbContext(DbContextOptions<ZonesDbContext> options) : DbContext(options)
{
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Street> Streets => Set<Street>();
    public DbSet<ZoneExclusion> ZoneExclusions => Set<ZoneExclusion>();
    public DbSet<StreetExclusion> StreetExclusions => Set<StreetExclusion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Zone>(builder =>
        {
            builder.ToTable("Zones");
            builder.HasKey(z => z.Id);
            builder.Property(z => z.Id)
                .HasConversion(id => id.Value, value => new ZoneId(value));
            builder.Property(z => z.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasMany(z => z.Streets)
                .WithOne()
                .HasForeignKey(s => s.ZoneId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(z => z.Exclusions)
                .WithOne()
                .HasForeignKey(e => e.ZoneId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Street>(builder =>
        {
            builder.ToTable("Streets");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id)
                .HasConversion(id => id.Value, value => new StreetId(value));
            builder.Property(s => s.ZoneId)
                .HasConversion(id => id.Value, value => new ZoneId(value));
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(s => s.RepresentsWholeZone)
                .IsRequired();

            builder.OwnsOne(s => s.PaidParkingSchedule, scheduleBuilder =>
            {
                scheduleBuilder.Property(p => p.StartTime)
                    .HasColumnName("PaidStartTime");
                scheduleBuilder.Property(p => p.EndTime)
                    .HasColumnName("PaidEndTime");
                scheduleBuilder.Property(p => p.PaidDays)
                    .HasColumnName("PaidDays")
                    .HasConversion(
                        days => string.Join(",", days.Select(d => (int)d)),
                        value => string.IsNullOrWhiteSpace(value)
                            ? Array.Empty<DayOfWeek>()
                            : value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => (DayOfWeek)int.Parse(s))
                                   .ToArray(),
                        new ValueComparer<DayOfWeek[]>(
                            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToArray()
                        )
                    );
            });
        });

        modelBuilder.Entity<ZoneExclusion>(builder =>
        {
            builder.ToTable("ZoneExclusions");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.ZoneId)
                .HasConversion(id => id.Value, value => new ZoneId(value));
            builder.Property(e => e.StartDate)
                .IsRequired();
            builder.Property(e => e.EndDate)
                .IsRequired();
            builder.Property(e => e.Reason)
                .IsRequired()
                .HasMaxLength(500);
        });

        modelBuilder.Entity<StreetExclusion>(builder =>
        {
            builder.ToTable("StreetExclusions");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.StreetId)
                .HasConversion(id => id.Value, value => new StreetId(value));
            builder.Property(e => e.StartDate)
                .IsRequired();
            builder.Property(e => e.EndDate)
                .IsRequired();
            builder.Property(e => e.Reason)
                .IsRequired()
                .HasMaxLength(500);
        });
    }
}
