namespace PTickets.Modules.InspectorTracking.Data;

using Microsoft.EntityFrameworkCore;
using PTickets.Shared;
using System.Reflection.Emit;

internal class InspectorTrackingDbContext(DbContextOptions<InspectorTrackingDbContext> options) : DbContext(options)
{
    public DbSet<Inspector> Inspectors => Set<Inspector>();
    public DbSet<LocationLog> LocationLogs => Set<LocationLog>();
    public DbSet<InspectionLog> InspectionLogs => Set<InspectionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Izolacja bazy: moduł dostaje własny schemat
        modelBuilder.HasDefaultSchema("inspector_tracking");

        modelBuilder.Entity<Inspector>(builder =>
        {
            builder.ToTable("Inspectors");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id)
                .HasConversion(id => id.Value, value => new InspectorId(value));

            builder.Property(i => i.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(i => i.LastName).IsRequired().HasMaxLength(100);
            builder.Property(i => i.AssignedToZone).IsRequired();

            builder.Property(i => i.ZoneId)
                .HasConversion(
                    id => id.HasValue ? id.Value.Value : (Guid?)null,
                    value => value.HasValue ? new ZoneId(value.Value) : null)
                .IsRequired(false);

            // Powiązania 1:N z backing fields i shadow property klucza obcego
            builder.HasMany(i => i.InspectionAttempts)
                .WithOne()
                .HasForeignKey("InspectorId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(i => i.InspectionAttempts)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(i => i.LocationLogs)
                .WithOne()
                .HasForeignKey("InspectorId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(i => i.LocationLogs)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<LocationLog>(builder =>
        {
            builder.ToTable("LocationLogs");
            builder.HasKey(l => l.Id);
            builder.Property(l => l.Id)
                .HasConversion(id => id.Value, value => new LocationLogId(value));

            builder.Property(l => l.TimeOfLocation).IsRequired();
            builder.Property(l => l.Latitude).HasPrecision(9, 6).IsRequired();
            builder.Property(l => l.Longitude).HasPrecision(9, 6).IsRequired();
        });

        modelBuilder.Entity<InspectionLog>(builder =>
        {
            builder.ToTable("InspectionLogs");
            builder.HasKey(il => il.Id);
            builder.Property(il => il.Id)
                .HasConversion(id => id.Value, value => new InspectionLogId(value));

            builder.Property(il => il.TimeOfAttempt).IsRequired();

            builder.Property(il => il.InspectionId)
                .HasConversion(
                    id => id.HasValue ? id.Value.Value : (Guid?)null,
                    value => value.HasValue ? new InspectionId(value.Value) : null)
                .IsRequired(false);

            builder.Property(il => il.LocationLogId)
                .HasConversion(
                    id => id.HasValue ? id.Value.Value : (Guid?)null,
                    value => value.HasValue ? new LocationLogId(value.Value) : null)
                .IsRequired(false);
        });
    }
}