namespace PTickets.Modules.Tickets.Application.Commands;

using MediatR;
using PTickets.Modules.Tickets.Domain;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public record RecordExternalTicketCommand(
    RegistrationNumber RegistrationNumber,
    StreetId StreetId,
    DateTime ValidFrom,
    DateTime ValidTo,
    string ProviderName) : IRequest<Guid>;

public class RecordExternalTicketHandler(ITicketRepository ticketRepository)
    : IRequestHandler<RecordExternalTicketCommand, Guid>
{
    public async Task<Guid> Handle(RecordExternalTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = Ticket.Create(
            request.RegistrationNumber,
            request.StreetId,
            request.ValidFrom,
            request.ValidTo,
            request.ProviderName);

        await ticketRepository.AddAsync(ticket, cancellationToken);
        await ticketRepository.SaveChangesAsync(cancellationToken);

        return ticket.Id;
    }
}

