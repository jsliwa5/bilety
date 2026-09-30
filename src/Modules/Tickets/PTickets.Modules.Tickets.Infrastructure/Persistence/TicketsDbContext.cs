namespace PTickets.Modules.Tickets.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Tickets.Domain;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public class TicketsDbContext(DbContextOptions<TicketsDbContext> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<ResidentCard> ResidentCards => Set<ResidentCard>();
    public DbSet<StreetZoneMapping> StreetZoneMappings => Set<StreetZoneMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ticket>(builder =>
        {
            builder.ToTable("Tickets", "tickets");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.ExternalTicketId)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(t => t.ParkingZone)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(t => t.RegistrationNumber)
                .HasConversion(rn => rn.Value, v => RegistrationNumber.Create(v))
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.StreetId)
                .HasConversion(id => id.HasValue ? id.Value.Value : Guid.Empty, value => value == Guid.Empty ? null : new StreetId(value))
                .IsRequired(false);

            builder.Property(t => t.ValidFrom)
                .IsRequired();

            builder.Property(t => t.ValidTo)
                .IsRequired();

            builder.Property(t => t.ProviderName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.CreatedAt)
                .IsRequired();
                
            builder.HasIndex(t => new { t.RegistrationNumber, t.StreetId, t.ValidTo });
            builder.HasIndex(t => t.ExternalTicketId).IsUnique();
        });

        modelBuilder.Entity<ResidentCard>(builder =>
        {
            builder.ToTable("ResidentCards", "tickets");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.RegistrationNumber)
                .HasConversion(rn => rn.Value, v => RegistrationNumber.Create(v))
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.StreetId)
                .HasConversion(id => id.Value, value => new StreetId(value))
                .IsRequired();

            builder.HasIndex(c => new { c.RegistrationNumber, c.StreetId, c.ValidTo });
        });

        modelBuilder.Entity<StreetZoneMapping>(builder =>
        {
            builder.ToTable("StreetZoneMappings", "tickets");
            builder.HasKey(m => m.StreetId);
            
            builder.Property(m => m.StreetId)
                .HasConversion(id => id.Value, value => new StreetId(value))
                .IsRequired();

            builder.Property(m => m.ZoneId)
                .HasConversion(id => id.Value, value => new ZoneId(value))
                .IsRequired();
        });
    }
}

