namespace PTickets.Modules.FileStorage.Domain;

using PTickets.Shared;

public class StoredFile
{
    private StoredFile()
    {
    }

    public FileId Id { get; private set; }
    public string OriginalName { get; private set; } = default!;
    public string StoragePath { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long SizeBytes { get; private set; }
    public DateTime UploadedAt { get; private set; }

    public static StoredFile Create(string originalName, string storagePath, string contentType, long sizeBytes)
    {
        return new StoredFile
        {
            Id = FileId.New(),
            OriginalName = originalName,
            StoragePath = storagePath,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            UploadedAt = DateTime.UtcNow
        };
    }
}

