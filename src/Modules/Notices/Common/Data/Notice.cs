using PTickets.Shared;
using PTickets.Shared.ValueObjects;
using PTickets.Modules.Notices.Common.Exceptions;

namespace PTickets.Modules.Notices.Common.Data;

public class Notice
{
    public NoticeId Id { get; private set; }
    public InspectionId InspectionId { get; private set; }
    public RegistrationNumber RegistrationNumber { get; private set; } = null!;
    public DateTime IssuedAt { get; private set; }
    public decimal PenaltyAmount { get; private set; }
    public decimal Surcharge { get; private set; }

    public decimal TotalAmount => PenaltyAmount + Surcharge;
    public NoticeStatus Status { get; private set; } = NoticeStatus.Issued;
    public DateTime? SyncedAt { get; private set; }

    private Notice() { }

    public static Notice Create(NoticeId id, InspectionId inspectionId, RegistrationNumber registrationNumber, decimal penaltyAmount, decimal surcharge, DateTime issuedAt)
    {
        return new Notice
        {
            Id = id,
            InspectionId = inspectionId,
            RegistrationNumber = registrationNumber,
            PenaltyAmount = penaltyAmount,
            Surcharge = surcharge,
            IssuedAt = issuedAt,
            Status = NoticeStatus.Issued,
            SyncedAt = null
        };
    }

    public void MarkAsSynced(DateTime syncedAt)
    {
        SyncedAt = syncedAt;
    }

    public void MarkAsPaid()
    {
        if (Status != NoticeStatus.Issued && Status != NoticeStatus.SentToDebtCollection)
        {
            throw new CannotPayNoticeException($"Cannot pay notice with status {Status}.");
        }

        Status = NoticeStatus.Paid;
    }

    public void Cancel()
    {
        if (Status != NoticeStatus.Issued && Status != NoticeStatus.Appealed)
        {
            throw new CannotCancelNoticeException($"Cannot cancel notice with status {Status}.");
        }

        Status = NoticeStatus.Cancelled;
    }

    public void MarkAsAppealed()
    {
        if (Status != NoticeStatus.Issued)
        {
            throw new InvalidNoticeStatusTransitionException($"Cannot appeal notice with status {Status}. Only Issued notices can be appealed.");
        }

        Status = NoticeStatus.Appealed;
    }

    public void RejectAppeal()
    {
        if (Status != NoticeStatus.Appealed)
        {
            throw new InvalidNoticeStatusTransitionException($"Cannot reject appeal for notice with status {Status}.");
        }

        Status = NoticeStatus.Issued;
    }

    public void SendToDebtCollection()
    {
        if (Status != NoticeStatus.Issued)
        {
            throw new InvalidNoticeStatusTransitionException($"Cannot send notice with status {Status} to debt collection.");
        }

        Status = NoticeStatus.SentToDebtCollection;
    }
}