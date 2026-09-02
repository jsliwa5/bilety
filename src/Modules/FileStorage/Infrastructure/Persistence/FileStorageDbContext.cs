namespace PTickets.Modules.FileStorage.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using PTickets.Modules.FileStorage.Domain;
using PTickets.Shared;

public class FileStorageDbContext(DbContextOptions<FileStorageDbContext> options) : DbContext(options)
{
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("file_storage");

        modelBuilder.Entity<StoredFile>(builder =>
        {
            builder.ToTable("StoredFiles");
            builder.HasKey(f => f.Id);
            builder.Property(f => f.Id)
                .HasConversion(id => id.Value, value => new FileId(value));
            builder.Property(f => f.OriginalName).IsRequired().HasMaxLength(500);
            builder.Property(f => f.StoragePath).IsRequired().HasMaxLength(1000);
            builder.Property(f => f.ContentType).IsRequired().HasMaxLength(100);
            builder.Property(f => f.SizeBytes).IsRequired();
            builder.Property(f => f.UploadedAt).IsRequired();
        });
    }
}

