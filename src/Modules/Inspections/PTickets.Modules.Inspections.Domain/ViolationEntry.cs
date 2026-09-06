using PTickets.Shared;

namespace PTickets.Modules.Inspections.Domain;

public class ViolationEntry
{
    public Guid Id { get; private set; }
    public InspectionId InspectionId { get; private set; }
    public ViolationTypeId ViolationTypeId { get; private set; }
    public ViolationSource Source { get; private set; }
    public DateTime AddedAt { get; private set; }

    private ViolationEntry() { }

    public static ViolationEntry Create(InspectionId inspectionId, ViolationTypeId violationTypeId, ViolationSource source)
    {
        return new ViolationEntry
        {
            Id = Guid.NewGuid(),
            InspectionId = inspectionId,
            ViolationTypeId = violationTypeId,
            Source = source,
            AddedAt = DateTime.UtcNow
        };
    }
}

