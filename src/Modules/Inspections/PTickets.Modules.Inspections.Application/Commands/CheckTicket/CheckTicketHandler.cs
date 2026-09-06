using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;
using PTickets.Shared.Contracts.Tickets;

namespace PTickets.Modules.Inspections.Application.Commands.CheckTicket;

public class CheckTicketHandler : IRequestHandler<CheckTicketCommand, TicketCheckResultDto>
{
    private readonly IInspectionRepository _repository;
    private readonly IMediator _mediator;

    public CheckTicketHandler(IInspectionRepository repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task<TicketCheckResultDto> Handle(CheckTicketCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _repository.GetByIdAsync(new InspectionId(request.InspectionId), cancellationToken)
            ?? throw new InvalidOperationException("Inspection not found.");

        var result = await _mediator.Send(new CheckRegistrationQuery(
            inspection.RegistrationNumber, 
            inspection.StreetId, 
            DateTime.UtcNow), cancellationToken);

        inspection.RecordTicketCheck(result, requiresSecondCheck: true);
        await _repository.SaveChangesAsync(cancellationToken);

        return new TicketCheckResultDto(result.IsValid, result.ProviderMessage);
    }
}

