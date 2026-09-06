using MediatR;

namespace PTickets.Modules.Inspections.Application.Queries.GetSession;

public record GetSessionQuery(Guid SessionId) : IRequest<SessionDto?>;

