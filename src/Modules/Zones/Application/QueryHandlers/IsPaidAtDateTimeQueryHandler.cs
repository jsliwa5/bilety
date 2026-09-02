namespace PTickets.Modules.Zones.Application.QueryHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Zones.Infrastructure.Persistence;
using PTickets.Shared.Contracts.Zones;

public class IsPaidAtDateTimeQueryHandler(ZonesDbContext dbContext) : IRequestHandler<IsPaidAtDateTimeQuery, bool>
{
    public async Task<bool> Handle(IsPaidAtDateTimeQuery request, CancellationToken cancellationToken)
    {
        var street = await dbContext.Streets
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.StreetId, cancellationToken);

        if (street is null)
            return false;

        return street.IsPaidAt(request.DateTime);
    }
}
