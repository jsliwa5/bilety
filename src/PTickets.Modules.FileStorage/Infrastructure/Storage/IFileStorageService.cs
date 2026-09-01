namespace PTickets.Modules.FileStorage.Infrastructure.Storage;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream content, string fileName, CancellationToken ct = default);
    Stream GetFileStream(string storagePath);
}

