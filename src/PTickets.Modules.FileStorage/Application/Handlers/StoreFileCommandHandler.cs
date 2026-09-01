namespace PTickets.Modules.FileStorage.Application.Handlers;

using MediatR;
using PTickets.Modules.FileStorage.Domain;
using PTickets.Modules.FileStorage.Infrastructure.Persistence;
using PTickets.Modules.FileStorage.Infrastructure.Storage;
using PTickets.Shared;
using PTickets.Shared.Contracts.FileStorage;

public class StoreFileCommandHandler(
    IFileStorageService fileStorageService,
    FileStorageDbContext dbContext) : IRequestHandler<StoreFileCommand, FileId>
{
    public async Task<FileId> Handle(StoreFileCommand request, CancellationToken cancellationToken)
    {
        var sizeBytes = request.Content.CanSeek ? request.Content.Length : 0;
        var storagePath = await fileStorageService.SaveFileAsync(request.Content, request.FileName, cancellationToken);

        if (sizeBytes == 0)
        {
            try
            {
                using var stream = fileStorageService.GetFileStream(storagePath);
                sizeBytes = stream.Length;
            }
            catch
            {
                // Fallback if stream length cannot be determined
            }
        }

        var storedFile = StoredFile.Create(request.FileName, storagePath, request.ContentType, sizeBytes);

        dbContext.StoredFiles.Add(storedFile);
        await dbContext.SaveChangesAsync(cancellationToken);

        return storedFile.Id;
    }
}

