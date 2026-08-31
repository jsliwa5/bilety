namespace PTickets.Modules.Zones.Application.QueryHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Zones.Infrastructure.Persistence;
using PTickets.Shared.Contracts.Zones;

public class StreetBelongsToZoneQueryHandler(ZonesDbContext dbContext) : IRequestHandler<StreetBelongsToZoneQuery, bool>
{
    public async Task<bool> Handle(StreetBelongsToZoneQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Streets.AnyAsync(
            s => s.Id == request.StreetId && s.ZoneId == request.ZoneId,
            cancellationToken);
    }
}
