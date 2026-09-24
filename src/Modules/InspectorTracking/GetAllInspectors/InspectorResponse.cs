namespace PTickets.Modules.InspectorTracking.GetAllInspectors;

public record InspectorResponse(
    Guid Id,
    string FirstName,
    string LastName,
    bool AssignedToZone,
    Guid? ZoneId);

