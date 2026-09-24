namespace PTickets.Modules.InspectorTracking;

using Microsoft.EntityFrameworkCore;
using PTickets.Modules.InspectorTracking.Contracts;
using PTickets.Shared;

internal class InspectorTrackingModuleFacade : IInspectorTrackingModule
{
    private readonly InspectorTrackingDbContext _db;

    public InspectorTrackingModuleFacade(InspectorTrackingDbContext db)
    {
        _db = db;
    }

    public async Task<bool> InspectorExistsAsync(InspectorId inspectorId, CancellationToken ct)
        => await _db.Inspectors.AnyAsync(i => i.Id == inspectorId, ct);

    public async Task<ZoneId?> GetAssignedZoneAsync(InspectorId inspectorId, CancellationToken ct)
    {
        var inspector = await _db.Inspectors
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == inspectorId, ct);

        return inspector?.ZoneId;
    }
}
