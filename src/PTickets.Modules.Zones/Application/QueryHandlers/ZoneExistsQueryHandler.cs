namespace PTickets.Modules.Zones.Application.QueryHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Zones.Infrastructure.Persistence;
using PTickets.Shared.Contracts.Zones;

public class ZoneExistsQueryHandler(ZonesDbContext dbContext) : IRequestHandler<ZoneExistsQuery, bool>
{
    public async Task<bool> Handle(ZoneExistsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Zones.AnyAsync(z => z.Id == request.ZoneId, cancellationToken);
    }
}
