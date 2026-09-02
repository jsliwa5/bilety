using MediatR;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.InspectorTracking.Data;
using PTickets.Shared;
using PTickets.Shared.Contracts.Inspections;

namespace PTickets.Modules.InspectorTracking.EventHandlers;

internal class InspectionStartedEventHandler(InspectorTrackingDbContext db)
    : INotificationHandler<InspectionStartedEvent>
{
    public async Task Handle(InspectionStartedEvent notification, CancellationToken ct)
    {
        var inspector = await db.Inspectors
            .FirstOrDefaultAsync(i => i.Id == notification.InspectorId, ct);

        if (inspector is null) return;

        var locationLog = new LocationLog(
            notification.InspectorId,
            notification.StartedAt,
            notification.Latitude,
            notification.Longitude);

        inspector.RegisterLocationLog(locationLog);

        var inspectionLog = new InspectionLog(
            notification.InspectionId,
            notification.StartedAt,
            locationLog.Id);

        inspector.RegisterInspectionAttempt(inspectionLog);

        await db.SaveChangesAsync(ct);
    }
}

