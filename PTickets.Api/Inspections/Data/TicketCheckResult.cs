namespace PTickets.Api.Inspections.Data;

public record TicketCheckResult(
    bool IsValid,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    string? TicketProviderMessage = null
);