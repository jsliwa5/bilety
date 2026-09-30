using PTickets.Shared;
using PTickets.Shared.ValueObjects;

namespace PTickets.Modules.Notices.Data;

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
            Status = NoticeStatus.Issued
        };
    }

    public void MarkAsPaid()
    {
        if (Status != NoticeStatus.Issued)
        {
            throw new InvalidOperationException($"Cannot pay notice with status {Status}. Only notices in Issued status can be paid.");
        }

        Status = NoticeStatus.Paid;
    }

    public void Cancel()
    {
        if (Status != NoticeStatus.Issued)
        {
            throw new InvalidOperationException($"Cannot cancel notice with status {Status}. Only notices in Issued status can be cancelled.");
        }

        Status = NoticeStatus.Cancelled;
    }
}

