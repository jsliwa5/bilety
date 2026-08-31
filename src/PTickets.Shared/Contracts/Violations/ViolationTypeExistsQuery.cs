namespace PTickets.Shared.Contracts.Violations;

using MediatR;

public record ViolationTypeExistsQuery(ViolationTypeId ViolationTypeId) : IRequest<bool>;
