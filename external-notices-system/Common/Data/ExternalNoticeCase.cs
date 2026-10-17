namespace ExternalNoticesSystem.Api.Common.Data;

public class ExternalNoticeCase
{
    public Guid NoticeId { get; private set; }
    public Guid InspectionId { get; private set; }
    public string RegistrationNumber { get; private set; } = string.Empty;
    public decimal BaseAmount { get; private set; }
    public decimal DiscountedAmount { get; private set; }
    public DateTime DiscountDeadline { get; private set; }
    public DateTime PaymentDueDate { get; private set; }
    public decimal? PaidAmount { get; private set; }
    public string Status { get; private set; } = "Issued";
    public string? AppealReason { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private ExternalNoticeCase() { }

    public static ExternalNoticeCase Create(
        Guid noticeId,
        Guid inspectionId,
        string registrationNumber,
        decimal totalAmount,
        string status,
        DateTime issuedAt)
    {
        return new ExternalNoticeCase
        {
            NoticeId = noticeId,
            InspectionId = inspectionId,
            RegistrationNumber = registrationNumber,
            BaseAmount = totalAmount,
            DiscountedAmount = Math.Round(totalAmount * 0.70m, 2),
            DiscountDeadline = issuedAt.AddDays(7),
            PaymentDueDate = issuedAt.AddDays(14),
            Status = status,
            IssuedAt = issuedAt,
            ReceivedAt = DateTime.UtcNow
        };
    }

    public bool IsDiscountActive(DateTime now) => now <= DiscountDeadline && Status == "Issued";

    public decimal GetCurrentAmountToPay(DateTime now)
    {
        if (Status is "Paid" or "Cancelled")
            return 0m;

        return IsDiscountActive(now) ? DiscountedAmount : BaseAmount;
    }

    public decimal Pay(DateTime now)
    {
        if (Status != "Issued" && Status != "SentToDebtCollection")
            throw new InvalidOperationException($"Nie można opłacić sprawy w statusie {Status}.");

        var amountToPay = GetCurrentAmountToPay(now);
        Status = "Paid";
        PaidAmount = amountToPay;
        UpdatedAt = now;
        return amountToPay;
    }

    public void Appeal(string reason, DateTime now)
    {
        if (Status != "Issued")
            throw new InvalidOperationException($"Odwołanie można złożyć tylko dla sprawy w statusie Issued (obecny: {Status}).");

        Status = "Appealed";
        AppealReason = reason;
        UpdatedAt = now;
    }

    public string ResolveAppeal(bool approved, DateTime now)
    {
        if (Status != "Appealed")
            throw new InvalidOperationException("Sprawa nie znajduje się w statusie odwołania (Appealed).");

        UpdatedAt = now;

        if (approved)
        {
            Status = "Cancelled";
            return "APPEAL_APPROVED";
        }

        Status = "Issued";
        return "APPEAL_REJECTED";
    }

    public void SendToDebtCollection(DateTime now)
    {
        if (Status != "Issued")
            throw new InvalidOperationException($"Nie można przekazać do windykacji sprawy w statusie {Status}.");

        Status = "SentToDebtCollection";
        UpdatedAt = now;
    }
}

