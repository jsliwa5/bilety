namespace PTickets.Modules.FileStorage.Infrastructure.Storage;

using Microsoft.Extensions.Configuration;

public class LocalFileStorageService(IConfiguration configuration) : IFileStorageService
{
    private readonly string _basePath = configuration["FileStorage:BasePath"] ?? "./uploads";

    public async Task<string> SaveFileAsync(Stream content, string fileName, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_basePath);

        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(_basePath, uniqueFileName);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fileStream, ct);

        return uniqueFileName;
    }

    public Stream GetFileStream(string storagePath)
    {
        var fullPath = Path.IsPathRooted(storagePath) ? storagePath : Path.Combine(_basePath, storagePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found at path: {storagePath}", fullPath);
        }

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}

