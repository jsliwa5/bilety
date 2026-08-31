namespace PTickets.Shared.ValueObjects;

public sealed record TicketCheckResult(
    bool IsValid,
    DateTime? ValidFrom = null,
    DateTime? ValidTo = null,
    string? ProviderMessage = null)
{
    public static TicketCheckResult Valid(DateTime validFrom, DateTime validTo, string? providerMessage = null) =>
        new(true, validFrom, validTo, providerMessage);

    public static TicketCheckResult Invalid(string? providerMessage = null) =>
        new(false, ProviderMessage: providerMessage);

    public static TicketCheckResult TicketNotRequired() =>
        new(true, ProviderMessage: "Ticket not required");
}
