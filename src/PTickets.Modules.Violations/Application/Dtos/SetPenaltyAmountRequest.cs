namespace PTickets.Modules.Violations.Application.Dtos;

public record SetPenaltyAmountRequest(decimal Amount, DateTime EffectiveFrom);
