using PTickets.Modules.Violations.Common.Exceptions;
namespace PTickets.Modules.Violations.Common.Data;

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
            throw new InvalidPenaltyAmountException();
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
