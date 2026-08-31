namespace PTickets.Modules.Violations.Application.QueryHandlers;

using MediatR;
using PTickets.Modules.Violations.Application.Services;
using PTickets.Shared.Contracts.Violations;

public class GetPenaltyAmountQueryHandler : IRequestHandler<GetPenaltyAmountQuery, decimal>
{
    private readonly PenaltyCalculationService _penaltyCalculationService;

    public GetPenaltyAmountQueryHandler(PenaltyCalculationService penaltyCalculationService)
    {
        _penaltyCalculationService = penaltyCalculationService;
    }

    public async Task<decimal> Handle(GetPenaltyAmountQuery request, CancellationToken cancellationToken)
    {
        return await _penaltyCalculationService.GetCurrentPenaltyAmountAsync(request.ViolationTypeId, cancellationToken);
    }
}
