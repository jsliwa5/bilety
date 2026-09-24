namespace PTickets.Modules.FileStorage;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTickets.Modules.FileStorage.Application;
using PTickets.Modules.FileStorage.Contracts;
using PTickets.Modules.FileStorage.Infrastructure.Persistence;
using PTickets.Modules.FileStorage.Infrastructure.Storage;

public static class FileStorageModule
{
    public static IServiceCollection AddFileStorageModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FileStorageConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=ptickets.db";
        services.AddDbContext<FileStorageDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IFileStorageModule, FileStorageModuleFacade>();

        return services;
    }
}

