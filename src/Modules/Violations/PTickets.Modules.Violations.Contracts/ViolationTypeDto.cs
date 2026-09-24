namespace PTickets.Modules.Violations.Contracts;

using PTickets.Shared;

public record ViolationTypeDto(
    ViolationTypeId Id,
    string Name,
    string? Description,
    decimal CurrentPenaltyAmount);
