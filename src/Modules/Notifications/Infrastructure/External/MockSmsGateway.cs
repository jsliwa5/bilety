namespace PTickets.Modules.Notifications.Infrastructure.External;

public class MockSmsGateway : ISmsGateway
{
    public Task<(bool Success, string? Error)> SendAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        Console.WriteLine($"[MockSmsGateway] Sending SMS to {phoneNumber}: {message}");
        return Task.FromResult((true, (string?)null));
    }
}

