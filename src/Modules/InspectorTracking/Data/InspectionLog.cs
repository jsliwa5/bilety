using PTickets.Shared;

namespace PTickets.Modules.InspectorTracking.Data;

internal record InspectionLog
{
    public InspectionLogId Id { get; init; }
    public InspectionId? InspectionId { get; init; }
    public DateTime TimeOfAttempt { get; init; }
    public LocationLogId? LocationLogId { get; init; }

    public InspectionLog(InspectionId? inspectionId, DateTime timeOfAttempt, LocationLogId? locationLogId)
    {
        Id = InspectionLogId.New();
        InspectionId = inspectionId;
        TimeOfAttempt = timeOfAttempt;
        LocationLogId = locationLogId;
    }
}