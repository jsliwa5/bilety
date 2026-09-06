using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;
using PTickets.Shared.Contracts.InspectorTracking;

namespace PTickets.Modules.Inspections.Application.Commands.StartSession;

public class StartSessionHandler : IRequestHandler<StartSessionCommand, Guid>
{
    private readonly ISessionRepository _repository;
    private readonly IMediator _mediator;

    public StartSessionHandler(ISessionRepository repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(StartSessionCommand request, CancellationToken cancellationToken)
    {
        var inspectorId = new InspectorId(request.InspectorId);
        
        var inspectorExists = await _mediator.Send(new InspectorExistsQuery(inspectorId), cancellationToken);
        if (!inspectorExists)
            throw new InvalidOperationException("Inspector does not exist.");
            
        var openSession = await _repository.GetOpenSessionForInspectorAsync(inspectorId, cancellationToken);
        if (openSession != null)
            throw new InvalidOperationException("Inspector already has an open session.");

        var session = Session.Create(inspectorId, DateTime.UtcNow);
        await _repository.AddAsync(session, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return session.Id.Value;
    }
}

