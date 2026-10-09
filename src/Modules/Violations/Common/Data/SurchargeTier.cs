using PTickets.Modules.Violations.Common.Exceptions;
namespace PTickets.Modules.Violations.Common.Data;

using PTickets.Shared;

public class SurchargeTier
{
    public PenaltyTierId Id { get; private set; }
    public int MinMinutes { get; private set; }
    public int? MaxMinutes { get; private set; }
    public decimal Amount { get; private set; }

    private SurchargeTier() { }

    public static SurchargeTier Create(int minMinutes, int? maxMinutes, decimal amount)
    {
        if (minMinutes < 0)
        {
            throw new InvalidSurchargeTierMinMinutesException();
        }

        if (amount <= 0)
        {
            throw new InvalidSurchargeTierAmountException();
        }

        if (maxMinutes.HasValue && maxMinutes.Value <= minMinutes)
        {
            throw new InvalidSurchargeTierMaxMinutesException();
        }

        return new SurchargeTier
        {
            Id = PenaltyTierId.New(),
            MinMinutes = minMinutes,
            MaxMinutes = maxMinutes,
            Amount = amount
        };
    }

    public bool Matches(int overtimeMinutes)
    {
        return overtimeMinutes >= MinMinutes && (!MaxMinutes.HasValue || overtimeMinutes <= MaxMinutes.Value);
    }
}
