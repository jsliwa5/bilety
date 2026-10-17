namespace ExternalNoticesSystem.Api.Common.Data;

using ExternalNoticesSystem.Api.Common.Outbox;
using Microsoft.EntityFrameworkCore;

public class ExternalNoticesDbContext(DbContextOptions<ExternalNoticesDbContext> options) : DbContext(options)
{
    public DbSet<ExternalNoticeCase> NoticeCases => Set<ExternalNoticeCase>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExternalNoticeCase>(b =>
        {
            b.HasKey(x => x.NoticeId);
            b.Property(x => x.NoticeId).ValueGeneratedNever();
            b.Property(x => x.RegistrationNumber).IsRequired().HasMaxLength(20);
            b.Property(x => x.Status).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Action).IsRequired().HasMaxLength(50);
            b.HasIndex(x => x.ProcessedAt);
        });
    }
}

