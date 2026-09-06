using PTickets.Shared;
using PTickets.Shared.ValueObjects;

namespace PTickets.Modules.Notices.Domain;

public class Notice
{
    public NoticeId Id { get; private set; }
    public InspectionId InspectionId { get; private set; }
    public RegistrationNumber RegistrationNumber { get; private set; } = null!;
    public DateTime IssuedAt { get; private set; }
    public decimal PenaltyAmount { get; private set; }
    public decimal Surcharge { get; private set; }
    
    public decimal TotalAmount => PenaltyAmount + Surcharge;

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
            IssuedAt = issuedAt
        };
    }
}

