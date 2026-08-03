using Microsoft.VisualBasic;
using PTickets.Api.Common;


namespace PTickets.Api.Penalties.Data;

public class Penalty
{
    public PenaltyId Id { get; private set; }
    public InspectionId InspectionId { get; private set; }
    public RegistrationNumber RegistrationNumber { get; private set; }
    public decimal Amount { get; private set; }
    public PenaltyStatus Status { get; private set; }
    public DateTime DueDate { get; private set; }

    private Penalty() { }

    public static Penalty Issue(InspectionId inspectionId, RegistrationNumber regNumber, decimal amount, DateTime dueDate)
    {
        if (amount <= 0)
            throw new ArgumentException("Kwota kary musi być większa niż 0.", nameof(amount));

        return new Penalty
        {
            Id = new PenaltyId(Guid.NewGuid()),
            InspectionId = inspectionId,
            RegistrationNumber = regNumber,
            Amount = amount,
            Status = PenaltyStatus.Issued,
            DueDate = dueDate
        };
    }

    // Metody biznesowe zmieniające stan agregatu:
    public void MarkAsPaid()
    {
        if (Status == PenaltyStatus.Cancelled)
            throw new InvalidOperationException("Nie można opłacić anulowanej kary.");

        Status = PenaltyStatus.Paid;
    }

    public void Cancel(string reason)
    {
        if (Status == PenaltyStatus.Paid)
            throw new InvalidOperationException("Nie można anulować opłaconej kary.");

        Status = PenaltyStatus.Cancelled;
    }
}
