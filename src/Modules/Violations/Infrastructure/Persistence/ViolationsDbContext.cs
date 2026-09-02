namespace PTickets.Modules.Violations.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Violations.Domain;
using PTickets.Shared;

public class ViolationsDbContext : DbContext
{
    public ViolationsDbContext(DbContextOptions<ViolationsDbContext> options) : base(options)
    {
    }

    public DbSet<ViolationType> ViolationTypes => Set<ViolationType>();
    public DbSet<PenaltyAmount> PenaltyAmounts => Set<PenaltyAmount>();
    public DbSet<SurchargeTier> SurchargeTiers => Set<SurchargeTier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ViolationType>(entity =>
        {
            entity.ToTable("violation_types");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasConversion(id => id.Value, value => new ViolationTypeId(value));
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasMany(e => e.PenaltyAmounts)
                .WithOne()
                .HasForeignKey(p => p.ViolationTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PenaltyAmount>(entity =>
        {
            entity.ToTable("penalty_amounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ViolationTypeId)
                .HasConversion(id => id.Value, value => new ViolationTypeId(value));
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.EffectiveFrom).IsRequired();
        });

        modelBuilder.Entity<SurchargeTier>(entity =>
        {
            entity.ToTable("surcharge_tiers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasConversion(id => id.Value, value => new PenaltyTierId(value));
            entity.Property(e => e.MinMinutes).IsRequired();
            entity.Property(e => e.MaxMinutes);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
        });
    }
}
