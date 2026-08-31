namespace PTickets.Shared.Contracts.Violations;

using MediatR;
using PTickets.Shared.Dtos;

public record GetAllViolationTypesQuery() : IRequest<List<ViolationTypeDto>>;
