namespace PTickets.Api.HealthChecks;

using global::HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

public static class HealthChecksExtensions
{
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // 1. PostgreSQL databases health checks
        RegisterDatabaseHealthCheck(healthChecksBuilder, configuration, "ZonesConnection", "postgres-zones");
        RegisterDatabaseHealthCheck(healthChecksBuilder, configuration, "ViolationsConnection", "postgres-violations");
        RegisterDatabaseHealthCheck(healthChecksBuilder, configuration, "TicketsConnection", "postgres-tickets");
        RegisterDatabaseHealthCheck(healthChecksBuilder, configuration, "InspectionsDb", "postgres-inspections");
        RegisterDatabaseHealthCheck(healthChecksBuilder, configuration, "NoticesModule", "postgres-notices");
        RegisterDatabaseHealthCheck(healthChecksBuilder, configuration, "InspectorTrackingConnection", "postgres-inspectortracking");
        RegisterDatabaseHealthCheck(healthChecksBuilder, configuration, "Notifications", "postgres-notifications");
        RegisterDatabaseHealthCheck(healthChecksBuilder, configuration, "FileStorageConnection", "postgres-filestorage");

        // 2. RabbitMQ health check
        var rabbitHost = configuration["RabbitMQ:HostName"] ?? "localhost";
        var rabbitPort = configuration["RabbitMQ:Port"] ?? "5672";
        var rabbitVHost = configuration["RabbitMQ:VirtualHost"] ?? "parking";
        var rabbitUser = configuration["RabbitMQ:UserName"] ?? "controller_app";
        var rabbitPass = configuration["RabbitMQ:Password"] ?? "CONTROLLER123";
        var rabbitUri = $"amqp://{rabbitUser}:{rabbitPass}@{rabbitHost}:{rabbitPort}/{rabbitVHost}";

        healthChecksBuilder.AddRabbitMQ(
            async sp =>
            {
                var factory = new RabbitMQ.Client.ConnectionFactory
                {
                    HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                    Port = int.TryParse(configuration["RabbitMQ:Port"], out var port) ? port : 5672,
                    VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "parking",
                    UserName = configuration["RabbitMQ:UserName"] ?? "controller_app",
                    Password = configuration["RabbitMQ:Password"] ?? "CONTROLLER123"
                };
                return await factory.CreateConnectionAsync();
            },
            name: "rabbitmq",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready", "messaging"]);

        return services;
    }

    private static void RegisterDatabaseHealthCheck(
        IHealthChecksBuilder builder, 
        IConfiguration configuration, 
        string connectionName, 
        string checkName)
    {
        var connectionString = configuration.GetConnectionString(connectionName) 
            ?? configuration.GetConnectionString("DefaultConnection");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            builder.AddNpgSql(
                connectionString,
                name: checkName,
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db"]);
        }
    }

    public static IEndpointRouteBuilder MapCustomHealthChecks(this IEndpointRouteBuilder app)
    {
        // Liveness probe: czy proces aplikacji żyje (brak odpytywania bazy i rabbita)
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        // Readiness probe: czy aplikacja jest gotowa przyjmować ruch (bazy + rabbitmq)
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Ogólny punkt diagnostyczny
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}
