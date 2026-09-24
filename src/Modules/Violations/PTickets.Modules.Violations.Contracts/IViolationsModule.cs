namespace PTickets.Modules.Violations.Contracts;

using PTickets.Shared;

public interface IViolationsModule
{
    Task<bool> ViolationTypeExistsAsync(ViolationTypeId id, CancellationToken ct = default);
    Task<decimal> GetPenaltyAmountAsync(ViolationTypeId id, CancellationToken ct = default);
    Task<decimal> CalculateSurchargeAsync(int overtimeMinutes, CancellationToken ct = default);
    Task<List<ViolationTypeDto>> GetAllViolationTypesAsync(CancellationToken ct = default);
}
