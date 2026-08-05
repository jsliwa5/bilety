using PTickets.Api.Common;
using PTickets.Api.Zones.Contract;


namespace PTickets.Api.Zones.Infrastructure;

public class FlatRatePenaltyCalculator : IPenaltyCalculator
{
    private const decimal FixedAmount = 100.00m;
    public decimal CalculateAmount(StreetId streetId, DateTime inspectionDate)
    {
        return FixedAmount;
    }
}
