using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;

namespace PTickets.Modules.Inspections.Application.Queries.GetSession;

public class GetSessionHandler : IRequestHandler<GetSessionQuery, SessionDto?>
{
    private readonly ISessionRepository _repository;

    public GetSessionHandler(ISessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<SessionDto?> Handle(GetSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await _repository.GetByIdAsync(new SessionId(request.SessionId), cancellationToken);
        if (session == null) return null;

        return new SessionDto(
            session.Id.Value,
            session.InspectorId.Value,
            session.SelectedZoneId?.Value,
            session.SelectedStreetId?.Value,
            session.StartedAt,
            session.ClosedAt);
    }
}

