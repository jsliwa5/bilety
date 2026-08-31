namespace PTickets.Modules.Violations.Application.Services;

using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Violations.Infrastructure.Persistence;
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
}
