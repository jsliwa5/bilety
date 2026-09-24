namespace PTickets.Modules.FileStorage.Application;

using Microsoft.EntityFrameworkCore;
using PTickets.Modules.FileStorage.Contracts;
using PTickets.Modules.FileStorage.Domain;
using PTickets.Modules.FileStorage.Infrastructure.Persistence;
using PTickets.Modules.FileStorage.Infrastructure.Storage;
using PTickets.Shared;

internal class FileStorageModuleFacade : IFileStorageModule
{
    private readonly IFileStorageService _fileStorageService;
    private readonly FileStorageDbContext _dbContext;

    public FileStorageModuleFacade(IFileStorageService fileStorageService, FileStorageDbContext dbContext)
    {
        _fileStorageService = fileStorageService;
        _dbContext = dbContext;
    }

    public async Task<FileId> StoreFileAsync(Stream content, string fileName, string contentType, CancellationToken ct)
    {
        var sizeBytes = content.CanSeek ? content.Length : 0;
        var storagePath = await _fileStorageService.SaveFileAsync(content, fileName, ct);

        if (sizeBytes == 0)
        {
            try
            {
                using var stream = _fileStorageService.GetFileStream(storagePath);
                sizeBytes = stream.Length;
            }
            catch
            {
                // Fallback if stream length cannot be determined
            }
        }

        var storedFile = StoredFile.Create(fileName, storagePath, contentType, sizeBytes);
        _dbContext.StoredFiles.Add(storedFile);
        await _dbContext.SaveChangesAsync(ct);

        return storedFile.Id;
    }

    public async Task<FileStreamResult> GetFileAsync(FileId fileId, CancellationToken ct)
    {
        var storedFile = await _dbContext.StoredFiles
            .FirstOrDefaultAsync(f => f.Id == fileId, ct);

        if (storedFile is null)
            throw new KeyNotFoundException($"File with id '{fileId}' was not found.");

        var stream = _fileStorageService.GetFileStream(storedFile.StoragePath);
        return new FileStreamResult(stream, storedFile.OriginalName, storedFile.ContentType);
    }
}
