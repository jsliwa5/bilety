using PTickets.Shared;
using PTickets.Shared.ValueObjects;

namespace PTickets.Modules.Inspections.Domain;

public class Notice
{
    public NoticeId Id { get; private set; }
    public InspectionId InspectionId { get; private set; }
    public RegistrationNumber RegistrationNumber { get; private set; } = null!;
    public DateTime IssuedAt { get; private set; }
    
    private readonly List<NoticeItem> _items = new();
    public IReadOnlyCollection<NoticeItem> Items => _items.AsReadOnly();
    
    public decimal TotalAmount => _items.Where(i => i.Status != NoticeItemStatus.Cancelled).Sum(i => i.Amount + i.Surcharge);

    private Notice() { }

    public static Notice Create(InspectionId inspectionId, RegistrationNumber registrationNumber, DateTime issuedAt)
    {
        return new Notice
        {
            Id = NoticeId.New(),
            InspectionId = inspectionId,
            RegistrationNumber = registrationNumber,
            IssuedAt = issuedAt
        };
    }
    
    public void AddItem(ViolationTypeId violationTypeId, decimal amount, decimal surcharge)
    {
        _items.Add(NoticeItem.Create(Id, violationTypeId, amount, surcharge));
    }
}

