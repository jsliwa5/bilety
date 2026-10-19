namespace PTickets.Modules.Violations;

using Microsoft.EntityFrameworkCore;
using PTickets.Shared;
using PTickets.Shared.Abstractions;

public class PenaltyCalculationService
{
    private readonly ViolationsDbContext _dbContext;
    private readonly IDateTimeProvider? _dateTimeProvider;

    public PenaltyCalculationService(ViolationsDbContext dbContext, IDateTimeProvider? dateTimeProvider = null)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<decimal> CalculateSurchargeAsync(int overtimeMinutes, CancellationToken cancellationToken = default)
    {
        var tiers = await _dbContext.SurchargeTiers
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var matchingTier = tiers.FirstOrDefault(t => t.Matches(overtimeMinutes));
        return matchingTier?.Amount ?? 0m;
    }

    public async Task<decimal> GetCurrentPenaltyAmountAsync(ViolationTypeId id, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider?.UtcNow ?? DateTime.UtcNow;

        var penalty = await _dbContext.PenaltyAmounts
            .AsNoTracking()
            .Where(p => p.ViolationTypeId == id && p.EffectiveFrom <= now)
            .OrderByDescending(p => p.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);

        return penalty?.Amount ?? 0m;
    }

    public async Task<decimal> CalculateTotalPenaltyAmountAsync(
        IEnumerable<ViolationTypeId> violationTypeIds,
        CancellationToken cancellationToken = default)
    {
        if (violationTypeIds is null)
            return 0m;

        var idList = violationTypeIds.ToList();
        if (idList.Count == 0)
            return 0m;

        var now = _dateTimeProvider?.UtcNow ?? DateTime.UtcNow;
        var distinctIds = idList.Distinct().ToList();

        var activePenalties = await _dbContext.PenaltyAmounts
            .AsNoTracking()
            .Where(p => distinctIds.Contains(p.ViolationTypeId) && p.EffectiveFrom <= now)
            .ToListAsync(cancellationToken);

        var currentRatesByType = activePenalties
            .GroupBy(p => p.ViolationTypeId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(p => p.EffectiveFrom).First().Amount);

        decimal total = 0m;
        foreach (var id in idList)
        {
            if (currentRatesByType.TryGetValue(id, out var rate))
            {
                total += rate;
            }
        }

        return total;
    }
}

