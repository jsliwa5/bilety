using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;

namespace PTickets.Modules.Inspections.Application.Commands.CloseSession;

public class CloseSessionHandler : IRequestHandler<CloseSessionCommand>
{
    private readonly ISessionRepository _repository;

    public CloseSessionHandler(ISessionRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(CloseSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _repository.GetByIdAsync(new SessionId(request.SessionId), cancellationToken)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.IsClosed)
            throw new InvalidOperationException("Session is already closed.");

        session.Close(DateTime.UtcNow);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

