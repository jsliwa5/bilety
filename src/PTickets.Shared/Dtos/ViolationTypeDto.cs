namespace PTickets.Shared.Dtos;

public record ViolationTypeDto(
    ViolationTypeId Id,
    string Name,
    string? Description,
    decimal CurrentPenaltyAmount);
