using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;
using PTickets.Shared.Contracts.Violations;

namespace PTickets.Modules.Inspections.Application.Commands.AddViolation;

public class AddViolationHandler : IRequestHandler<AddViolationCommand>
{
    private readonly IInspectionRepository _repository;
    private readonly IMediator _mediator;

    public AddViolationHandler(IInspectionRepository repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task Handle(AddViolationCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _repository.GetByIdAsync(new InspectionId(request.InspectionId), cancellationToken)
            ?? throw new InvalidOperationException("Inspection not found.");

        if (inspection.Status != InspectionStatus.AwaitingDecision && inspection.Status != InspectionStatus.ViolationFound)
            throw new InvalidOperationException("Cannot add violation in current state.");

        var violationTypeId = new ViolationTypeId(request.ViolationTypeId);
        var typeExists = await _mediator.Send(new ViolationTypeExistsQuery(violationTypeId), cancellationToken);
        if (!typeExists)
            throw new InvalidOperationException("Violation type does not exist.");

        inspection.AddVisualViolation(violationTypeId);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

