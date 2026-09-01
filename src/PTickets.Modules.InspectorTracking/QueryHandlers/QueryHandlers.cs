using MediatR;
using Microsoft.EntityFrameworkCore;
using PTickets.Shared;
using PTickets.Shared.Contracts.InspectorTracking;

namespace PTickets.Modules.InspectorTracking.QueryHandlers;

internal class InspectorExistsQueryHandler(InspectorTrackingDbContext db)
    : IRequestHandler<InspectorExistsQuery, bool>
{
    public async Task<bool> Handle(InspectorExistsQuery request, CancellationToken ct)
        => await db.Inspectors.AnyAsync(i => i.Id == request.InspectorId, ct);
}

internal class GetAssignedZoneQueryHandler(InspectorTrackingDbContext db)
    : IRequestHandler<GetAssignedZoneQuery, ZoneId?>
{
    public async Task<ZoneId?> Handle(GetAssignedZoneQuery request, CancellationToken ct)
    {
        var inspector = await db.Inspectors
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.InspectorId, ct);

        return inspector?.ZoneId;
    }
}

