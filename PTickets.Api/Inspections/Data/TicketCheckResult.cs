namespace PTickets.Api.Inspections.Data;

public record TicketCheckResult
{
    public bool IsValid { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidTo { get; init; }
    public string? TicketProviderMessage { get; init; }

    public TicketCheckResult()
    {
    }

    public static TicketCheckResult TicketNotRequired()
    {
        return new TicketCheckResult(true, null, null, "Payment not required in this zone at this time.");
    }

    public TicketCheckResult(bool isValid, DateTime? validFrom, DateTime? validTo, string? ticketProviderMessage)
    {
        IsValid = isValid;
        ValidFrom = validFrom;
        ValidTo = validTo;
        TicketProviderMessage = ticketProviderMessage;
    }
}
    
