namespace PTickets.Modules.FileStorage;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTickets.Modules.FileStorage.Infrastructure.Persistence;
using PTickets.Modules.FileStorage.Infrastructure.Storage;

public static class FileStorageModule
{
    public static IServiceCollection AddFileStorageModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FileStorageDbContext>((sp, options) =>
        {
            if (!options.IsConfigured)
            {
                var connectionString = configuration.GetConnectionString("FileStorageConnection")
                    ?? configuration.GetConnectionString("DefaultConnection")
                    ?? configuration.GetConnectionString("Database")
                    ?? "Data Source=ptickets.db";
            }
        });

        services.AddSingleton<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}

