namespace PTickets.Modules.Zones.Application.QueryHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Zones.Infrastructure.Persistence;
using PTickets.Shared.Contracts.Zones;

public class IsPaidAtDateTimeQueryHandler(ZonesDbContext dbContext) : IRequestHandler<IsPaidAtDateTimeQuery, bool>
{
    public async Task<bool> Handle(IsPaidAtDateTimeQuery request, CancellationToken cancellationToken)
    {
        var query = from s in dbContext.Streets
                    join z in dbContext.Zones on s.ZoneId equals z.Id
                    where s.Id == request.StreetId
                    select new { StreetSchedule = s.PaidParkingSchedule, ZoneSchedule = z.PaidParkingSchedule };

        var result = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        if (result is null)
            return false;

        var schedule = result.StreetSchedule ?? result.ZoneSchedule;
        
        return schedule?.IsPaidAt(request.DateTime) ?? false;
    }
}
