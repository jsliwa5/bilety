using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;
using PTickets.Modules.Inspections.Contracts.Events;
using PTickets.Shared.ValueObjects;

namespace PTickets.Modules.Inspections.Application.Commands.StartInspection;

public class StartInspectionHandler : IRequestHandler<StartInspectionCommand, Guid>
{
    private readonly IInspectionRepository _inspectionRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IMediator _mediator;

    public StartInspectionHandler(
        IInspectionRepository inspectionRepository,
        ISessionRepository sessionRepository,
        IMediator mediator)
    {
        _inspectionRepository = inspectionRepository;
        _sessionRepository = sessionRepository;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(StartInspectionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(new SessionId(request.SessionId), cancellationToken)
            ?? throw new InvalidOperationException("Session not found.");
            
        if (session.IsClosed)
            throw new InvalidOperationException("Session is closed.");
            
        if (session.SelectedZoneId == null || session.SelectedStreetId == null)
            throw new InvalidOperationException("Street and zone must be selected before starting inspection.");

        var inspection = Inspection.Create(
            session.Id,
            session.InspectorId,
            new RegistrationNumber(request.RegistrationNumber),
            session.SelectedZoneId.Value,
            session.SelectedStreetId.Value,
            request.Latitude,
            request.Longitude,
            DateTime.UtcNow);

        await _inspectionRepository.AddAsync(inspection, cancellationToken);
        await _inspectionRepository.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new InspectionStartedEvent(
            inspection.Id,
            inspection.InspectorId,
            inspection.Latitude,
            inspection.Longitude,
            inspection.StartedAt), cancellationToken);

        return inspection.Id.Value;
    }
}

