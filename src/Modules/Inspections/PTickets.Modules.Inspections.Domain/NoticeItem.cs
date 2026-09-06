using PTickets.Shared;

namespace PTickets.Modules.Inspections.Domain;

public class NoticeItem
{
    public Guid Id { get; private set; }
    public NoticeId NoticeId { get; private set; }
    public ViolationTypeId ViolationTypeId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Surcharge { get; private set; }
    public NoticeItemStatus Status { get; private set; } = NoticeItemStatus.Issued;

    private NoticeItem() { }

    public static NoticeItem Create(NoticeId noticeId, ViolationTypeId violationTypeId, decimal amount, decimal surcharge)
    {
        return new NoticeItem
        {
            Id = Guid.NewGuid(),
            NoticeId = noticeId,
            ViolationTypeId = violationTypeId,
            Amount = amount,
            Surcharge = surcharge,
            Status = NoticeItemStatus.Issued
        };
    }
}

