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
    public DateTime? TimeOfFirstCheck { get; private set; }
    public DateTime? TimeOfSecondCheck { get; private set; }
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

    public void SelectZoneAndStreet(ZoneId zoneId, StreetId streetId)
    {
        ZoneId = zoneId;
        StreetId = streetId;
    }

    public void RecordTicketCheck(TicketCheckResult result, bool requiresSecondCheck = true)
    {
        if (Status == InspectionStatus.AwaitingDecision)
        {
            TicketResult = result;
            TimeOfFirstCheck = DateTime.UtcNow;

            if (result.IsValid)
            {
                Status = InspectionStatus.Approved;
            }
            else
            {
                if (requiresSecondCheck)
                {
                    Status = InspectionStatus.AwaitingSecondCheck;
                }
                else
                {
                    Violations.Add(ViolationEntry.Create(Id, ViolationTypeId.NoTicket, ViolationSource.TicketCheck));
                    Status = InspectionStatus.ViolationFound;
                }
            }
        }
        else if (Status == InspectionStatus.AwaitingSecondCheck)
        {
            if (_photoIds.Count == 0)
            {
                throw new PTickets.Modules.Inspections.Domain.Exceptions.MissingFirstCheckPhotosException();
            }

            SecondCheckResult = result;
            TimeOfSecondCheck = DateTime.UtcNow;

            if (result.IsValid)
            {
                Status = InspectionStatus.Approved;
            }
            else
            {
                Violations.Add(ViolationEntry.Create(Id, ViolationTypeId.NoTicket, ViolationSource.TicketCheck));
                Status = InspectionStatus.ViolationFound;
            }
        }
        else
        {
            throw new PTickets.Modules.Inspections.Domain.Exceptions.InvalidInspectionStateException("Ticket cannot be checked when inspection is already approved or violation already found!");
        }
    }

    [Obsolete("Use RecordTicketCheck instead. This method will be removed in a future version.")]
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
        if (Status == InspectionStatus.AwaitingDecision || Status == InspectionStatus.AwaitingSecondCheck)
        {
            Violations.Add(ViolationEntry.Create(Id, typeId, ViolationSource.Visual));
            Status = InspectionStatus.ViolationFound;
        }
        else
        {
            throw new PTickets.Modules.Inspections.Domain.Exceptions.InvalidInspectionStateException("Cannot add visual violation when inspection is already approved or violation already found!");
        }
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
        if (fileIds == null || fileIds.Count == 0)
            throw new PTickets.Modules.Inspections.Domain.Exceptions.NoPhotosProvidedException();

        if (Status != InspectionStatus.AwaitingSecondCheck && Status != InspectionStatus.ViolationFound)
        {
            throw new PTickets.Modules.Inspections.Domain.Exceptions.InvalidInspectionStateException("Photos can only be attached when awaiting second check or when a violation is found.");
        }

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
            throw new PTickets.Modules.Inspections.Domain.Exceptions.CannotApproveInspectionWithViolationsException();

        Status = InspectionStatus.Approved;
    }
}

