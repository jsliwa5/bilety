namespace PTickets.Modules.Inspections.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Inspections.Domain;

public class InspectionsDbContext : DbContext
{
    public InspectionsDbContext(DbContextOptions<InspectionsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Inspection> Inspections => Set<Inspection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inspections");
        base.OnModelCreating(modelBuilder);
    }
}

