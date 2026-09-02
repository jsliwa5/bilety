namespace PTickets.Modules.Violations.Domain;

using PTickets.Shared;

public class PenaltyAmount
{
    public Guid Id { get; private set; }
    public ViolationTypeId ViolationTypeId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime EffectiveFrom { get; private set; }

    private PenaltyAmount() { }

    public static PenaltyAmount Create(ViolationTypeId violationTypeId, decimal amount, DateTime effectiveFrom)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Penalty amount must be greater than zero.");
        }

        return new PenaltyAmount
        {
            Id = Guid.NewGuid(),
            ViolationTypeId = violationTypeId,
            Amount = amount,
            EffectiveFrom = effectiveFrom
        };
    }
}
