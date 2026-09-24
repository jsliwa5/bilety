using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Modules.InspectorTracking.Contracts;
using PTickets.Shared;

namespace PTickets.Modules.Inspections.Application.Commands.StartSession;

public class StartSessionHandler : IRequestHandler<StartSessionCommand, Guid>
{
    private readonly ISessionRepository _repository;
    private readonly IInspectorTrackingModule _inspectorTracking;

    public StartSessionHandler(ISessionRepository repository, IInspectorTrackingModule inspectorTracking)
    {
        _repository = repository;
        _inspectorTracking = inspectorTracking;
    }

    public async Task<Guid> Handle(StartSessionCommand request, CancellationToken cancellationToken)
    {
        var inspectorId = new InspectorId(request.InspectorId);
        
        var inspectorExists = await _inspectorTracking.InspectorExistsAsync(inspectorId, cancellationToken);
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

