namespace PTickets.Modules.Violations;

using Microsoft.EntityFrameworkCore;

using PTickets.Modules.Violations.Contracts;
using PTickets.Modules.Violations;
using PTickets.Shared;
using PTickets.Shared.Abstractions;

internal class ViolationsModuleFacade : IViolationsModule
{
    private readonly PenaltyCalculationService _penaltyService;
    private readonly ViolationsDbContext _dbContext;
    private readonly IDateTimeProvider? _dateTimeProvider;

    public ViolationsModuleFacade(
        PenaltyCalculationService penaltyService,
        ViolationsDbContext dbContext,
        IDateTimeProvider? dateTimeProvider = null)
    {
        _penaltyService = penaltyService;
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<bool> ViolationTypeExistsAsync(ViolationTypeId id, CancellationToken ct)
        => await _dbContext.ViolationTypes.AsNoTracking().AnyAsync(v => v.Id == id, ct);

    public async Task<decimal> GetPenaltyAmountAsync(ViolationTypeId id, CancellationToken ct)
        => await _penaltyService.GetCurrentPenaltyAmountAsync(id, ct);

    public async Task<decimal> CalculateSurchargeAsync(int overtimeMinutes, CancellationToken ct)
        => await _penaltyService.CalculateSurchargeAsync(overtimeMinutes, ct);

    public async Task<List<ViolationTypeDto>> GetAllViolationTypesAsync(CancellationToken ct)
    {
        var now = _dateTimeProvider?.UtcNow ?? DateTime.UtcNow;

        var violationTypes = await _dbContext.ViolationTypes
            .Include(v => v.PenaltyAmounts)
            .AsNoTracking()
            .ToListAsync(ct);

        return violationTypes.Select(v =>
        {
            var currentPenalty = v.PenaltyAmounts
                .Where(p => p.EffectiveFrom <= now)
                .OrderByDescending(p => p.EffectiveFrom)
                .Select(p => p.Amount)
                .FirstOrDefault();

            return new ViolationTypeDto(v.Id.Value, v.Name, v.Description, currentPenalty);
        }).ToList();
    }
}
