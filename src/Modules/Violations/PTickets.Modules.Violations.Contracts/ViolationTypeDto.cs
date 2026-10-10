namespace PTickets.Modules.Violations.Contracts;

using PTickets.Shared;

public record ViolationTypeDto(
    Guid Id,
    string Name,
    string? Description,
    decimal CurrentPenaltyAmount);
