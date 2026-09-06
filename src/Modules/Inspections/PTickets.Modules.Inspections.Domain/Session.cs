using PTickets.Shared;

namespace PTickets.Modules.Inspections.Domain;

public class Session
{
    public SessionId Id { get; private set; }
    public InspectorId InspectorId { get; private set; }
    public ZoneId? SelectedZoneId { get; private set; }
    public StreetId? SelectedStreetId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    
    public bool IsClosed => ClosedAt.HasValue;

    private Session() { }

    public static Session Create(InspectorId inspectorId, DateTime startedAt)
    {
        return new Session
        {
            Id = SessionId.New(),
            InspectorId = inspectorId,
            StartedAt = startedAt
        };
    }
    
    public void SelectStreetAndZone(ZoneId zoneId, StreetId streetId)
    {
        SelectedZoneId = zoneId;
        SelectedStreetId = streetId;
    }
    
    public void Close(DateTime closedAt)
    {
        ClosedAt = closedAt;
    }
}

