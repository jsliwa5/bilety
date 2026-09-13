namespace PTickets.Modules.Notifications.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Notifications.Domain;
using PTickets.Shared.ValueObjects;

public class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }

    public DbSet<PhoneRegistration> PhoneRegistrations => Set<PhoneRegistration>();
    public DbSet<SmsLog> SmsLogs => Set<SmsLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("notifications");

        modelBuilder.Entity<PhoneRegistration>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RegistrationNumber)
                .HasConversion(
                    rn => rn.Value,
                    v => new RegistrationNumber(v))
                .IsRequired();
            builder.Property(x => x.PhoneNumber)
                .IsRequired();
            builder.Property(x => x.RegisteredAt)
                .IsRequired();
        });

        modelBuilder.Entity<SmsLog>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PhoneNumber)
                .IsRequired();
            builder.Property(x => x.Message)
                .IsRequired();
            builder.Property(x => x.SentAt)
                .IsRequired();
            builder.Property(x => x.Success)
                .IsRequired();
            builder.Property(x => x.ErrorMessage);
        });
    }
}

