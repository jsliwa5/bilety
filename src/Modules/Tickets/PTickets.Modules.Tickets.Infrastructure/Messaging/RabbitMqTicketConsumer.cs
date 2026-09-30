namespace PTickets.Modules.Tickets.Infrastructure.Messaging;

using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PTickets.Modules.Tickets.Domain;
using PTickets.Shared.ValueObjects;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class RabbitMqTicketConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqTicketConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqTicketConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<RabbitMqTicketConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = /*_configuration["RabbitMQ:HostName"] ?? */ "172.0.0.1",
            Port = /*int.TryParse(_configuration["RabbitMQ:Port"], out var port) ? port : */ 5672,
            VirtualHost = /*_configuration["RabbitMQ:VirtualHost"] ?? */ "parking",
            //UserName = _configuration["RabbitMQ:UserName"] ?? "controller_app",
            //Password = _configuration["RabbitMQ:Password"] ?? "CONTROLLER123"
            UserName = "controller_app",
            Password = "CONTROLLER123"
        };

        try
        {
            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

            var queueName = _configuration["RabbitMQ:QueueName"] ?? "parking.controller-sync";

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var messageStr = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<RabbitMqTicketMessage>(messageStr);

                    if (message == null)
                    {
                        _logger.LogWarning("Failed to deserialize message: {Message}", messageStr);
                        await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                        return;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var ticketRepository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();

                    var exists = await ticketRepository.ExistsByExternalIdAsync(message.TicketId, stoppingToken);
                    if (!exists)
                    {
                        var ticket = Ticket.CreateFromExternal(
                            message.TicketId,
                            RegistrationNumber.Create(message.LicensePlate),
                            message.ValidFrom,
                            message.ValidTo,
                            message.Provider,
                            message.ParkingZone);

                        await ticketRepository.AddAsync(ticket, stoppingToken);
                        await ticketRepository.SaveChangesAsync(stoppingToken);
                        
                        _logger.LogInformation("Saved external ticket {TicketId} for registration {Registration}", message.TicketId, message.LicensePlate);
                    }
                    else
                    {
                        _logger.LogInformation("Ticket {TicketId} already exists, skipping", message.TicketId);
                    }

                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    
                    if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.TryGetValue("retry_count", out var retryObj))
                    {
                        var retryCount = retryObj is int r ? r : 0;
                        if (retryCount < 3)
                        {
                            var properties = new BasicProperties();
                            if (ea.BasicProperties.Headers != null)
                            {
                                foreach (var header in ea.BasicProperties.Headers)
                                {
                                    properties.Headers ??= new Dictionary<string, object?>();
                                    properties.Headers[header.Key] = header.Value;
                                }
                            }
                            properties.Headers ??= new Dictionary<string, object?>();
                            properties.Headers["retry_count"] = retryCount + 1;
                            
                            // Re-publish to retry exchange
                            try 
                            {
                                await _channel.BasicPublishAsync(
                                    exchange: "parking.tickets.retry-exchange",
                                    routingKey: "",
                                    mandatory: false,
                                    basicProperties: properties,
                                    body: ea.Body,
                                    cancellationToken: stoppingToken);
                                    
                                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                                return;
                            }
                            catch (Exception pubEx)
                            {
                                _logger.LogError(pubEx, "Failed to publish to retry exchange");
                            }
                        }
                    }

                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            
            _logger.LogInformation("RabbitMQ Consumer started listening on {Queue}", queueName);
            
            // Keep the task alive
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ Consumer");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ Consumer...");
        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
        }
        if (_connection != null)
        {
            await _connection.CloseAsync(cancellationToken);
        }
        await base.StopAsync(cancellationToken);
    }
}
