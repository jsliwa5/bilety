namespace PTickets.Modules.Tickets.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Tickets.Domain;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public class TicketsDbContext(DbContextOptions<TicketsDbContext> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ticket>(builder =>
        {
            builder.ToTable("Tickets", "tickets");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.RegistrationNumber)
                .HasConversion(rn => rn.Value, v => RegistrationNumber.Create(v))
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.StreetId)
                .HasConversion(id => id.Value, value => new StreetId(value))
                .IsRequired();

            builder.Property(t => t.ValidFrom)
                .IsRequired();

            builder.Property(t => t.ValidTo)
                .IsRequired();

            builder.Property(t => t.ProviderName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.CreatedAt)
                .IsRequired();
        });
    }
}

