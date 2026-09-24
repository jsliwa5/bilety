namespace PTickets.Modules.InspectorTracking.Contracts;

using PTickets.Shared;

public interface IInspectorTrackingModule
{
    Task<bool> InspectorExistsAsync(InspectorId inspectorId, CancellationToken ct = default);
    Task<ZoneId?> GetAssignedZoneAsync(InspectorId inspectorId, CancellationToken ct = default);
}
