using PTickets.Shared;
using PTickets.Shared.ValueObjects;

namespace PTickets.Modules.Inspections.Domain;

public class Inspection
{
    public InspectionId Id { get; private set; }
    public SessionId SessionId { get; private set; }
    public InspectorId InspectorId { get; private set; }
    public RegistrationNumber RegistrationNumber { get; private set; } = null!;
    public ZoneId ZoneId { get; private set; }
    public StreetId StreetId { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public DateTime StartedAt { get; private set; }
    public InspectionStatus Status { get; private set; }
    
    public TicketCheckResult? TicketResult { get; private set; }
    public TicketCheckResult? SecondCheckResult { get; private set; }
    
    public List<ViolationEntry> Violations { get; private set; } = new();
    
    private readonly List<FileId> _photoIds = new();
    public IReadOnlyCollection<FileId> PhotoIds => _photoIds.AsReadOnly();
    
    public NoticeId? NoticeId { get; private set; }

    private Inspection() { }

    public static Inspection Create(SessionId sessionId, InspectorId inspectorId, RegistrationNumber registrationNumber, ZoneId zoneId, StreetId streetId, double lat, double lng, DateTime startedAt)
    {
        return new Inspection
        {
            Id = InspectionId.New(),
            SessionId = sessionId,
            InspectorId = inspectorId,
            RegistrationNumber = registrationNumber,
            ZoneId = zoneId,
            StreetId = streetId,
            Latitude = lat,
            Longitude = lng,
            StartedAt = startedAt,
            Status = InspectionStatus.AwaitingDecision
        };
    }
    
    public void RecordTicketCheck(TicketCheckResult result, bool requiresSecondCheck = false)
    {
        TicketResult = result;
        if (result.IsValid)
        {
            //Status = InspectionStatus.Approved;
        }
        else
        {
            Violations.Add(ViolationEntry.Create(Id, ViolationTypeId.NoTicket, ViolationSource.TicketCheck));
            Status = requiresSecondCheck ? InspectionStatus.AwaitingSecondCheck : InspectionStatus.ViolationFound;
        }
    }
    
    public void RecordSecondCheck(TicketCheckResult result)
    {
        SecondCheckResult = result;
        if (result.IsValid)
        {
            Violations.RemoveAll(v => v.Source == ViolationSource.TicketCheck);
            Status = InspectionStatus.Approved;
        }
        else
        {
            Status = InspectionStatus.ViolationFound;
        }
    }
    
    public void AddVisualViolation(ViolationTypeId typeId)
    {
        Violations.Add(ViolationEntry.Create(Id, typeId, ViolationSource.Visual));
        Status = InspectionStatus.ViolationFound;
    }
    
    public void RemoveViolation(Guid violationEntryId)
    {
        Violations.RemoveAll(v => v.Id == violationEntryId);
        if (Violations.Count == 0 && Status == InspectionStatus.ViolationFound)
        {
            Status = InspectionStatus.Approved;
        }
    }
    
    public void AttachPhotos(List<FileId> fileIds)
    {
        _photoIds.AddRange(fileIds);
        if (Status == InspectionStatus.ViolationFound)
        {
            Status = InspectionStatus.PhotosAttached;
        }
    }
    
    public void MarkNoticeIssued(NoticeId noticeId)
    {
        NoticeId = noticeId;
        Status = InspectionStatus.NoticeIssued;
    }
    
    public void Approve()
    {
        if (Violations.Count > 0)
            throw new InvalidOperationException("Cannot approve an inspection with violations.");
            
        Status = InspectionStatus.Approved;
    }
}

