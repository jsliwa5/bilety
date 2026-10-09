using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;
using PTickets.Modules.Inspections.Contracts.Events;
using PTickets.Shared.ValueObjects;
using PTickets.Modules.Notices.Contracts;

namespace PTickets.Modules.Inspections.Application.Commands.StartInspection;

public class StartInspectionHandler : IRequestHandler<StartInspectionCommand, Guid>
{
    private readonly IInspectionRepository _inspectionRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IMediator _mediator;
    private readonly INoticesModule _noticesModule;

    public StartInspectionHandler(
        IInspectionRepository inspectionRepository,
        ISessionRepository sessionRepository,
        IMediator mediator,
        INoticesModule noticesModule)
    {
        _inspectionRepository = inspectionRepository;
        _sessionRepository = sessionRepository;
        _mediator = mediator;
        _noticesModule = noticesModule;
    }

    public async Task<Guid> Handle(StartInspectionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(new SessionId(request.SessionId), cancellationToken)
            ?? throw new PTickets.Modules.Inspections.Domain.Exceptions.SessionNotFoundException();

        if (session.IsClosed)
            throw new PTickets.Modules.Inspections.Domain.Exceptions.SessionAlreadyClosedException();

        var wasNoticeIssuedForVehicleToday = await _noticesModule.WasNoticeIssuedForDateAsync(
            new RegistrationNumber(request.RegistrationNumber),
            DateTime.UtcNow.Date,
            cancellationToken);

        if (wasNoticeIssuedForVehicleToday)
            throw new PTickets.Modules.Inspections.Domain.Exceptions.NoticeAlreadyIssuedTodayException();

        var inspectionAwaitingForSecondCheck = await _inspectionRepository.GetInspectionAwaitingForSecondCheckAsync(
            new RegistrationNumber(request.RegistrationNumber), DateTime.UtcNow,
            cancellationToken);

        Inspection inspection = null;

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
                new RegistrationNumber(request.RegistrationNumber),
                new ZoneId(request.ZoneId),
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

