namespace PTickets.Shared.Contracts.Violations;

using MediatR;

public record CalculateSurchargeQuery(int OvertimeMinutes) : IRequest<decimal>;
