namespace PTickets.Modules.Violations.Domain;

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
            throw new ArgumentOutOfRangeException(nameof(minMinutes), "MinMinutes must be greater than or equal to 0.");
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than 0.");
        }

        if (maxMinutes.HasValue && maxMinutes.Value <= minMinutes)
        {
            throw new ArgumentException("MaxMinutes must be greater than MinMinutes.", nameof(maxMinutes));
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
