using PTickets.Api.Common;

namespace PTickets.Api.Zones.Contract;

public interface IPenaltyCalculator
{
    public decimal CalculateAmount(StreetId streetId, DateTime inspectionDate);
}
