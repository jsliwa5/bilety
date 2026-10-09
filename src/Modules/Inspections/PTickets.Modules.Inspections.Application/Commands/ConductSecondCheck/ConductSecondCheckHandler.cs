using MediatR;
using PTickets.Modules.Inspections.Application.Commands.CheckTicket;
using PTickets.Modules.Inspections.Domain;
using PTickets.Modules.Tickets.Contracts;
using PTickets.Shared;

namespace PTickets.Modules.Inspections.Application.Commands.ConductSecondCheck;

[Obsolete("Use CheckTicketHandler instead. The second check is now handled automatically via StartInspection + CheckTicket flow.")]
public class ConductSecondCheckHandler : IRequestHandler<ConductSecondCheckCommand, TicketCheckResultDto>
{
    private readonly IInspectionRepository _repository;
    private readonly ITicketsModule _tickets;

    public ConductSecondCheckHandler(IInspectionRepository repository, ITicketsModule tickets)
    {
        _repository = repository;
        _tickets = tickets;
    }

    public async Task<TicketCheckResultDto> Handle(ConductSecondCheckCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _repository.GetByIdAsync(new InspectionId(request.InspectionId), cancellationToken)
            ?? throw new PTickets.Modules.Inspections.Domain.Exceptions.InspectionNotFoundException();

        if (inspection.Status != InspectionStatus.AwaitingSecondCheck)
            throw new PTickets.Modules.Inspections.Domain.Exceptions.InvalidInspectionStateException("Inspection is not awaiting second check.");

        var result = await _tickets.CheckRegistrationAsync(
            inspection.RegistrationNumber, 
            inspection.StreetId, 
            DateTime.UtcNow, cancellationToken);

        inspection.RecordSecondCheck(result);
        await _repository.SaveChangesAsync(cancellationToken);

        return new TicketCheckResultDto(result.IsValid, result.ProviderMessage);
    }
}

