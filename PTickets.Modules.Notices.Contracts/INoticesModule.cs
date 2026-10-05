using PTickets.Shared.ValueObjects;

namespace PTickets.Modules.Notices.Contracts;

public interface INoticesModule
{
    Task<bool> WasNoticeIssuedForDateAsync(RegistrationNumber registrationNumber, DateTime date, CancellationToken cancellationToken);

}
    