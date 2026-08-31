namespace PTickets.Modules.Violations.Application.QueryHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Violations.Infrastructure.Persistence;
using PTickets.Shared.Abstractions;
using PTickets.Shared.Contracts.Violations;
using PTickets.Shared.Dtos;

public class GetAllViolationTypesQueryHandler : IRequestHandler<GetAllViolationTypesQuery, List<ViolationTypeDto>>
{
    private readonly ViolationsDbContext _dbContext;
    private readonly IDateTimeProvider? _dateTimeProvider;

    public GetAllViolationTypesQueryHandler(ViolationsDbContext dbContext, IDateTimeProvider? dateTimeProvider = null)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<List<ViolationTypeDto>> Handle(GetAllViolationTypesQuery request, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider?.UtcNow ?? DateTime.UtcNow;

        var violationTypes = await _dbContext.ViolationTypes
            .Include(v => v.PenaltyAmounts)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return violationTypes.Select(v =>
        {
            var currentPenalty = v.PenaltyAmounts
                .Where(p => p.EffectiveFrom <= now)
                .OrderByDescending(p => p.EffectiveFrom)
                .Select(p => p.Amount)
                .FirstOrDefault();

            return new ViolationTypeDto(v.Id, v.Name, v.Description, currentPenalty);
        }).ToList();
    }
}
