namespace PTickets.Modules.Notifications.Infrastructure.External;

public interface ISmsGateway
{
    Task<(bool Success, string? Error)> SendAsync(string phoneNumber, string message, CancellationToken ct = default);
}

