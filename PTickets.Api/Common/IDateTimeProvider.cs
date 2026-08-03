namespace PTickets.Api.Common;

public interface IDateTimeProvider
{
    public DateTime UtcNow { get; }

}
