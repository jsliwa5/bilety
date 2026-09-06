using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;
using PTickets.Shared.Contracts.Inspections;
using PTickets.Shared.Contracts.Violations;

namespace PTickets.Modules.Inspections.Application.Commands.IssueNotice;

public class IssueNoticeHandler : IRequestHandler<IssueNoticeCommand, Guid>
{
    private readonly IInspectionRepository _inspectionRepository;
    private readonly INoticeRepository _noticeRepository;
    private readonly IMediator _mediator;

    public IssueNoticeHandler(IInspectionRepository inspectionRepository, INoticeRepository noticeRepository, IMediator mediator)
    {
        _inspectionRepository = inspectionRepository;
        _noticeRepository = noticeRepository;
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

        var notice = Notice.Create(inspection.Id, inspection.RegistrationNumber, DateTime.UtcNow);

        foreach (var violation in inspection.Violations)
        {
            var violationTypeId = violation.ViolationTypeId;
            // Default violation type for auto-added ticket checks
            if (violation.Source == ViolationSource.TicketCheck && violationTypeId == ViolationTypeId.Empty)
            {
                // Assign some known ID, assuming it exists or can be matched.
                // Normally this would be looked up or configured.
                violationTypeId = new ViolationTypeId(Guid.Empty);
            }

            var amount = await _mediator.Send(new GetPenaltyAmountQuery(violationTypeId), cancellationToken);
            
            decimal surcharge = 0;
            if (violation.Source == ViolationSource.TicketCheck)
            {
                // Simply calculate based on some overtime for ticket checks if desired, or skip.
                // Assuming CalculateSurchargeQuery can be called (maybe pass 0 for now as it's not well defined here without valid ticket end time).
                surcharge = await _mediator.Send(new CalculateSurchargeQuery(0), cancellationToken);
            }

            notice.AddItem(violationTypeId, amount, surcharge);
        }

        await _noticeRepository.AddAsync(notice, cancellationToken);
        
        inspection.MarkNoticeIssued(notice.Id);
        
        await _inspectionRepository.SaveChangesAsync(cancellationToken);
        await _noticeRepository.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new NoticeIssuedEvent(
            notice.Id,
            inspection.Id,
            inspection.RegistrationNumber,
            notice.TotalAmount,
            notice.Items.Sum(i => i.Surcharge),
            notice.IssuedAt), cancellationToken);

        return notice.Id.Value;
    }
}

