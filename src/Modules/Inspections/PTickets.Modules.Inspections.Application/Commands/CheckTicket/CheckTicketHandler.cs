using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Modules.Tickets.Contracts;
using PTickets.Shared;

namespace PTickets.Modules.Inspections.Application.Commands.CheckTicket;

public class CheckTicketHandler : IRequestHandler<CheckTicketCommand, TicketCheckResultDto>
{
    private readonly IInspectionRepository _repository;
    private readonly ITicketsModule _tickets;

    public CheckTicketHandler(IInspectionRepository repository, ITicketsModule tickets)
    {
        _repository = repository;
        _tickets = tickets;
    }

    public async Task<TicketCheckResultDto> Handle(CheckTicketCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _repository.GetByIdAsync(new InspectionId(request.InspectionId), cancellationToken)
            ?? throw new InvalidOperationException("Inspection not found.");

        var result = await _tickets.CheckRegistrationAsync(
            inspection.RegistrationNumber, 
            inspection.StreetId, 
            DateTime.UtcNow, cancellationToken);

        inspection.RecordTicketCheck(result);
        await _repository.SaveChangesAsync(cancellationToken);

        return new TicketCheckResultDto(result.IsValid, result.ProviderMessage);
    }
}

