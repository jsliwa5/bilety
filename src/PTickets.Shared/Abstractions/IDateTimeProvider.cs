namespace PTickets.Shared.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
