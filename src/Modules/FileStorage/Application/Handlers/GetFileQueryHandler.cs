namespace PTickets.Modules.FileStorage.Application.Handlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.FileStorage.Infrastructure.Persistence;
using PTickets.Modules.FileStorage.Infrastructure.Storage;
using PTickets.Shared.Contracts.FileStorage;

public class GetFileQueryHandler(
    FileStorageDbContext dbContext,
    IFileStorageService fileStorageService) : IRequestHandler<GetFileQuery, FileStreamResult>
{
    public async Task<FileStreamResult> Handle(GetFileQuery request, CancellationToken cancellationToken)
    {
        var storedFile = await dbContext.StoredFiles
            .FirstOrDefaultAsync(f => f.Id == request.FileId, cancellationToken);

        if (storedFile is null)
        {
            throw new KeyNotFoundException($"File with id '{request.FileId}' was not found.");
        }

        var stream = fileStorageService.GetFileStream(storedFile.StoragePath);

        return new FileStreamResult(stream, storedFile.OriginalName, storedFile.ContentType);
    }
}

