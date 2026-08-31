namespace PTickets.Modules.Zones.Application.QueryHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Zones.Infrastructure.Persistence;
using PTickets.Shared.Contracts.Zones;

public class IsExcludedQueryHandler(ZonesDbContext dbContext) : IRequestHandler<IsExcludedQuery, bool>
{
    public async Task<bool> Handle(IsExcludedQuery request, CancellationToken cancellationToken)
    {
        var isZoneExcluded = await dbContext.ZoneExclusions
            .AnyAsync(e => e.ZoneId == request.ZoneId && request.DateTime >= e.StartDate && request.DateTime <= e.EndDate, cancellationToken);

        if (isZoneExcluded)
            return true;

        var isStreetExcluded = await dbContext.StreetExclusions
            .AnyAsync(e => e.StreetId == request.StreetId && request.DateTime >= e.StartDate && request.DateTime <= e.EndDate, cancellationToken);

        return isStreetExcluded;
    }
}
