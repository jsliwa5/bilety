using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Notices.Common.Data;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

namespace PTickets.Modules.Notices;

public class NoticesDbContext : DbContext
{
    public NoticesDbContext(DbContextOptions<NoticesDbContext> options) : base(options) { }

    public DbSet<Notice> Notices { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notices");

        modelBuilder.Entity<Notice>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasConversion(x => x.Value, x => new NoticeId(x));
            b.Property(x => x.InspectionId).HasConversion(x => x.Value, x => new InspectionId(x));
            b.Property(x => x.RegistrationNumber).HasConversion(x => x.Value, x => new RegistrationNumber(x));
            b.Property(x => x.PenaltyAmount);
            b.Property(x => x.Surcharge);
            b.Property(x => x.Status).HasConversion<string>();
        });
    }
}

