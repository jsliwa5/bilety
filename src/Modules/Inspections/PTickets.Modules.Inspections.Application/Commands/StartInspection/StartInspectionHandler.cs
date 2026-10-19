using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;
using PTickets.Modules.Inspections.Contracts.Events;
using PTickets.Shared.ValueObjects;
using PTickets.Modules.Notices.Contracts;
using PTickets.Modules.InspectorTracking.Contracts;
using PTickets.Modules.Inspections.Domain.Exceptions;

namespace PTickets.Modules.Inspections.Application.Commands.StartInspection;

public class StartInspectionHandler : IRequestHandler<StartInspectionCommand, Guid>
{
    private readonly IInspectionRepository _inspectionRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IMediator _mediator;
    private readonly INoticesModule _noticesModule;
    private readonly IInspectorTrackingModule _inspectorTracking;

    public StartInspectionHandler(
        IInspectionRepository inspectionRepository,
        ISessionRepository sessionRepository,
        IMediator mediator,
        INoticesModule noticesModule,
        IInspectorTrackingModule inspectorTracking)
    {
        _inspectionRepository = inspectionRepository;
        _sessionRepository = sessionRepository;
        _mediator = mediator;
        _noticesModule = noticesModule;
        _inspectorTracking = inspectorTracking;
    }

    public async Task<Guid> Handle(StartInspectionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(new SessionId(request.SessionId), cancellationToken)
            ?? throw new SessionNotFoundException();

        if (session.IsClosed)
            throw new SessionAlreadyClosedException();

        var requestedZoneId = new ZoneId(request.ZoneId);
        var assignedZoneId = await _inspectorTracking.GetAssignedZoneAsync(session.InspectorId, cancellationToken);
        if (assignedZoneId.HasValue && assignedZoneId.Value != requestedZoneId)
            throw new InspectorNotAssignedToZoneException();

        var registrationNumber = new RegistrationNumber(request.RegistrationNumber);

        var wasNoticeIssuedForVehicleToday = await _noticesModule.WasNoticeIssuedForDateAsync(
            registrationNumber,
            DateTime.UtcNow.Date,
            cancellationToken);

        if (wasNoticeIssuedForVehicleToday)
            throw new NoticeAlreadyIssuedTodayException();

        var inspectionAwaitingForSecondCheck = await _inspectionRepository.GetInspectionAwaitingForSecondCheckAsync(
            registrationNumber,
            DateTime.UtcNow,
            cancellationToken);

        Inspection inspection;

        if (inspectionAwaitingForSecondCheck is not null)
        {
            inspection = inspectionAwaitingForSecondCheck;

            await _mediator.Publish(new InspectionStartedEvent(
                inspection.Id,
                session.InspectorId,
                request.Latitude,
                request.Longitude,
                DateTime.UtcNow), cancellationToken);
        }
        else
        {
            inspection = Inspection.Create(
                session.Id,
                session.InspectorId,
                registrationNumber,
                requestedZoneId,
                new StreetId(request.StreetId),
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
        }

        return inspection.Id.Value;
    }
}

