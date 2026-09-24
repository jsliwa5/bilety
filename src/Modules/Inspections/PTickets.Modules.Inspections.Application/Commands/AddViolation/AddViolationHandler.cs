using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Modules.Violations.Contracts;
using PTickets.Shared;

namespace PTickets.Modules.Inspections.Application.Commands.AddViolation;

public class AddViolationHandler : IRequestHandler<AddViolationCommand>
{
    private readonly IInspectionRepository _repository;
    private readonly IViolationsModule _violations;

    public AddViolationHandler(IInspectionRepository repository, IViolationsModule violations)
    {
        _repository = repository;
        _violations = violations;
    }

    public async Task Handle(AddViolationCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _repository.GetByIdAsync(new InspectionId(request.InspectionId), cancellationToken)
            ?? throw new InvalidOperationException("Inspection not found.");

        if (inspection.Status != InspectionStatus.AwaitingDecision && inspection.Status != InspectionStatus.ViolationFound)
            throw new InvalidOperationException("Cannot add violation in current state.");

        var violationTypeId = new ViolationTypeId(request.ViolationTypeId);
        var typeExists = await _violations.ViolationTypeExistsAsync(violationTypeId, cancellationToken);
        if (!typeExists)
            throw new InvalidOperationException("Violation type does not exist.");

        inspection.AddVisualViolation(violationTypeId);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

