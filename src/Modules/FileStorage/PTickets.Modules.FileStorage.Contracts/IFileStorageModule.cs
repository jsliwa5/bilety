namespace PTickets.Modules.FileStorage.Contracts;

using PTickets.Shared;

public interface IFileStorageModule
{
    Task<FileId> StoreFileAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);
    Task<FileStreamResult> GetFileAsync(FileId fileId, CancellationToken ct = default);
}
