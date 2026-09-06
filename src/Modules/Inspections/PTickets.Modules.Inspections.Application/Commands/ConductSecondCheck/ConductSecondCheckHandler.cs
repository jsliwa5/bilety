using MediatR;
using PTickets.Modules.Inspections.Application.Commands.CheckTicket;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;
using PTickets.Shared.Contracts.Tickets;

namespace PTickets.Modules.Inspections.Application.Commands.ConductSecondCheck;

public class ConductSecondCheckHandler : IRequestHandler<ConductSecondCheckCommand, TicketCheckResultDto>
{
    private readonly IInspectionRepository _repository;
    private readonly IMediator _mediator;

    public ConductSecondCheckHandler(IInspectionRepository repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task<TicketCheckResultDto> Handle(ConductSecondCheckCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _repository.GetByIdAsync(new InspectionId(request.InspectionId), cancellationToken)
            ?? throw new InvalidOperationException("Inspection not found.");

        if (inspection.Status != InspectionStatus.AwaitingSecondCheck)
            throw new InvalidOperationException("Inspection is not awaiting second check.");

        var result = await _mediator.Send(new CheckRegistrationQuery(
            inspection.RegistrationNumber, 
            inspection.StreetId, 
            DateTime.UtcNow), cancellationToken);

        inspection.RecordSecondCheck(result);
        await _repository.SaveChangesAsync(cancellationToken);

        return new TicketCheckResultDto(result.IsValid, result.ProviderMessage);
    }
}

