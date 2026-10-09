using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Modules.Violations.Contracts;
using PTickets.Shared;
using PTickets.Modules.Inspections.Contracts.Events;

namespace PTickets.Modules.Inspections.Application.Commands.IssueNotice;

public class IssueNoticeHandler : IRequestHandler<IssueNoticeCommand, Guid>
{
    private readonly IInspectionRepository _inspectionRepository;
    private readonly IViolationsModule _violations;
    private readonly IMediator _mediator;

    public IssueNoticeHandler(IInspectionRepository inspectionRepository, IViolationsModule violations, IMediator mediator)
    {
        _inspectionRepository = inspectionRepository;
        _violations = violations;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(IssueNoticeCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _inspectionRepository.GetByIdAsync(new InspectionId(request.InspectionId), cancellationToken)
            ?? throw new PTickets.Modules.Inspections.Domain.Exceptions.InspectionNotFoundException();

        if (inspection.Status != InspectionStatus.PhotosAttached)
            throw new PTickets.Modules.Inspections.Domain.Exceptions.MissingPhotosForNoticeException();

        if (inspection.Violations.Count == 0)
            throw new PTickets.Modules.Inspections.Domain.Exceptions.NoViolationsForNoticeException();

        decimal totalPenaltyAmount = 0;
        decimal totalSurcharge = 0;

        foreach (var violation in inspection.Violations)
        {
            var violationTypeId = violation.ViolationTypeId;
            // Default violation type for auto-added ticket checks
            if (violation.Source == ViolationSource.TicketCheck && violationTypeId == ViolationTypeId.Empty)
            {
                violationTypeId = ViolationTypeId.NoTicket;
            }

            var amount = await _violations.GetPenaltyAmountAsync(violationTypeId, cancellationToken);
            
            decimal surcharge = 0;
            if (violation.Source == ViolationSource.TicketCheck)
            {
                surcharge = await _violations.CalculateSurchargeAsync(0, cancellationToken);
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

