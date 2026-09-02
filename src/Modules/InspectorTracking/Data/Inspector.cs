using PTickets.Shared;

namespace PTickets.Modules.InspectorTracking.Data;

internal class Inspector
{
  

    public InspectorId Id { get; set; }
    public String? FirstName { get; set; }
    public String? LastName { get; set; }
    public bool AssignedToZone { get; set; }
    public ZoneId? ZoneId { get; set; }
    private readonly List<InspectionLog> _inspectionAttempts = [];
    public IReadOnlyCollection<InspectionLog> InspectionAttempts => _inspectionAttempts.AsReadOnly();
    private readonly List<LocationLog> _locationLogs = [];
    public IReadOnlyCollection<LocationLog> LocationLogs => _locationLogs.AsReadOnly();

    private Inspector() // EF Core
    {
    }

    //for creating new
    private Inspector(string firstName, string lastName)
    {
        Id = InspectorId.New();
        FirstName = firstName;
        AssignedToZone = false;
        ZoneId = null;
        LastName = lastName;
    }

    //for restoring
    private Inspector(InspectorId id, string firstName, string lastName, bool assignedToZone, ZoneId? zoneId, IEnumerable<LocationLog> locationLogs,IEnumerable<InspectionLog> inspectionLogs)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        AssignedToZone = assignedToZone;
        ZoneId = zoneId;
        _locationLogs.AddRange(locationLogs);
        _inspectionAttempts.AddRange(inspectionLogs);
    }

    public static Inspector Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Imię inspektora nie może być puste.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Nazwisko inspektora nie może być puste.", nameof(lastName));
        return new Inspector(firstName.Trim(), lastName.Trim());
    }

    public static Inspector Restore(InspectorId id, string firstName, string lastName, bool assignedToZone, ZoneId? zoneId, IEnumerable<LocationLog> locationLogs, IEnumerable<InspectionLog> inspectionLogs)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("Id inspektora nie może być puste.", nameof(id));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Imię inspektora nie może być puste.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Nazwisko inspektora nie może być puste.", nameof(lastName));
        return new Inspector(id, firstName.Trim(), lastName.Trim(), assignedToZone, zoneId, locationLogs, inspectionLogs);
    }

    public void AssignToZone(ZoneId zoneId)
    {
        ZoneId = zoneId;
        AssignedToZone = true;
    }

    public void UnassignFromZone()
    {
        ZoneId = null;
        AssignedToZone = false;
    }

    public void RegisterInspectionAttempt(InspectionLog inspectionLog)
    {
        if (inspectionLog == null)
            throw new ArgumentNullException(nameof(inspectionLog));
        _inspectionAttempts.Add(inspectionLog);
    }

    public void RegisterLocationLog(LocationLog locationLog)
    {
        if (locationLog == null)
            throw new ArgumentNullException(nameof(locationLog));
        _locationLogs.Add(locationLog);
    }



}
