namespace PTickets.Shared.Contracts.Violations;

using MediatR;

public record GetPenaltyAmountQuery(ViolationTypeId ViolationTypeId) : IRequest<decimal>;
