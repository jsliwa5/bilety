using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;
using PTickets.Shared.Contracts.Inspections;
using PTickets.Shared.Contracts.Violations;

namespace PTickets.Modules.Inspections.Application.Commands.IssueNotice;

public class IssueNoticeHandler : IRequestHandler<IssueNoticeCommand, Guid>
{
    private readonly IInspectionRepository _inspectionRepository;
    private readonly IMediator _mediator;

    public IssueNoticeHandler(IInspectionRepository inspectionRepository, IMediator mediator)
    {
        _inspectionRepository = inspectionRepository;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(IssueNoticeCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _inspectionRepository.GetByIdAsync(new InspectionId(request.InspectionId), cancellationToken)
            ?? throw new InvalidOperationException("Inspection not found.");

        if (inspection.Status != InspectionStatus.PhotosAttached)
            throw new InvalidOperationException("Must attach photos before issuing a notice.");

        if (inspection.Violations.Count == 0)
            throw new InvalidOperationException("No violations found to issue a notice for.");

        decimal totalPenaltyAmount = 0;
        decimal totalSurcharge = 0;

        foreach (var violation in inspection.Violations)
        {
            var violationTypeId = violation.ViolationTypeId;
            // Default violation type for auto-added ticket checks
            if (violation.Source == ViolationSource.TicketCheck && violationTypeId == ViolationTypeId.Empty)
            {
                violationTypeId = new ViolationTypeId(Guid.Empty);
            }

            var amount = await _mediator.Send(new GetPenaltyAmountQuery(violationTypeId), cancellationToken);
            
            decimal surcharge = 0;
            if (violation.Source == ViolationSource.TicketCheck)
            {
                surcharge = await _mediator.Send(new CalculateSurchargeQuery(0), cancellationToken);
            }

            totalPenaltyAmount += amount;
            totalSurcharge += surcharge;
        }

        var noticeId = NoticeId.New();
        inspection.MarkNoticeIssued(noticeId);
        
        await _inspectionRepository.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new NoticeIssuedEvent(
            noticeId,
            inspection.Id,
            inspection.RegistrationNumber,
            totalPenaltyAmount,
            totalSurcharge,
            DateTime.UtcNow), cancellationToken);

        return noticeId.Value;
    }
}

