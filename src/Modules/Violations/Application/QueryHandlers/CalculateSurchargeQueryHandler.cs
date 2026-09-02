namespace PTickets.Modules.Violations.Application.QueryHandlers;

using MediatR;
using PTickets.Modules.Violations.Application.Services;
using PTickets.Shared.Contracts.Violations;

public class CalculateSurchargeQueryHandler : IRequestHandler<CalculateSurchargeQuery, decimal>
{
    private readonly PenaltyCalculationService _penaltyCalculationService;

    public CalculateSurchargeQueryHandler(PenaltyCalculationService penaltyCalculationService)
    {
        _penaltyCalculationService = penaltyCalculationService;
    }

    public async Task<decimal> Handle(CalculateSurchargeQuery request, CancellationToken cancellationToken)
    {
        return await _penaltyCalculationService.CalculateSurchargeAsync(request.OvertimeMinutes, cancellationToken);
    }
}
