using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;

namespace PTickets.Modules.Inspections.Application.Commands.ApproveInspection;

public class ApproveInspectionHandler : IRequestHandler<ApproveInspectionCommand>
{
    private readonly IInspectionRepository _repository;

    public ApproveInspectionHandler(IInspectionRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(ApproveInspectionCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _repository.GetByIdAsync(new InspectionId(request.InspectionId), cancellationToken)
            ?? throw new InvalidOperationException("Inspection not found.");

        inspection.Approve();
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

