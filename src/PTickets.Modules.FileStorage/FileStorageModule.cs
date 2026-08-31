using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PTickets.Modules.FileStorage;

public static class FileStorageModule
{
    public static IServiceCollection AddFileStorageModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}

