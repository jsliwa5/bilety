using PTickets.Api.Common;
using PTickets.Api.Zones.Contract;
using PTickets.Api.Zones.Data;

namespace PTickets.Api.Zones.Infrastructure;

public sealed class ZoneFacade : IZoneFacade
{
    private readonly IZoneRepository _zoneRepository;
    private readonly IStreetRepository _streetRepository;
    private readonly IPenaltyCalculator _penaltyCalculator;

    public ZoneFacade(
        IZoneRepository zoneRepository,
        IStreetRepository streetRepository,
        IPenaltyCalculator penaltyCalculator)
    {
        _zoneRepository = zoneRepository;
        _streetRepository = streetRepository;
        _penaltyCalculator = penaltyCalculator;
    }

    public Task<bool> ExistsByIdAsync(
        ZoneId zoneId)
        => _zoneRepository.ExistsAsync(zoneId);

    public async Task<bool> StreetBelongsToZoneAsync(
        ZoneId zoneId,
        StreetId streetId)
    {
        var street = await _streetRepository.GetAsync(streetId);

        return street is not null &&
               street.ZoneId == zoneId;
    }

    public async Task<bool> IsPaidAtDateTimeAsync(
        StreetId streetId,
        DateTime dateTime)
    {
        var street = await _streetRepository.GetAsync(streetId);

        if (street is null)
            throw new InvalidOperationException(
                $"Street '{streetId}' does not exist.");

        return street.IsPaid(dateTime);
    }

    public async Task<decimal> CalculatePenaltyAmountAsync(StreetId streetid, DateTime inspectionDate)
    {
        return await Task.FromResult(_penaltyCalculator.CalculateAmount(streetid, inspectionDate));
    }
}