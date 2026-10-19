namespace PTickets.Modules.Inspections.Tests;

using FluentAssertions;
using MediatR;
using Moq;
using PTickets.Modules.Inspections.Application.Commands.AddViolation;
using PTickets.Modules.Inspections.Application.Commands.CloseSession;
using PTickets.Modules.Inspections.Application.Commands.IssueNotice;
using PTickets.Modules.Inspections.Application.Commands.StartInspection;
using PTickets.Modules.Inspections.Application.Commands.StartSession;
using PTickets.Modules.Inspections.Contracts.Events;
using PTickets.Modules.Inspections.Domain;
using PTickets.Modules.Inspections.Domain.Exceptions;
using PTickets.Modules.InspectorTracking.Contracts;
using PTickets.Modules.Notices.Contracts;
using PTickets.Modules.Violations.Contracts;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;
using Xunit;

public class InspectionAndSessionHandlersTests
{
    private readonly Mock<ISessionRepository> _sessionRepoMock = new();
    private readonly Mock<IInspectionRepository> _inspectionRepoMock = new();
    private readonly Mock<IInspectorTrackingModule> _inspectorTrackingMock = new();
    private readonly Mock<INoticesModule> _noticesModuleMock = new();
    private readonly Mock<IViolationsModule> _violationsModuleMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();

    [Fact]
    public async Task StartSession_WhenInspectorHasOpenSession_ThrowsOpenSessionAlreadyExistsException()
    {
        var inspectorId = InspectorId.New();
        _inspectorTrackingMock.Setup(x => x.InspectorExistsAsync(inspectorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _sessionRepoMock.Setup(x => x.GetOpenSessionForInspectorAsync(inspectorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Session.Create(inspectorId, DateTime.UtcNow));

        var handler = new StartSessionHandler(_sessionRepoMock.Object, _inspectorTrackingMock.Object);

        await Assert.ThrowsAsync<OpenSessionAlreadyExistsException>(() =>
            handler.Handle(new StartSessionCommand(inspectorId.Value), CancellationToken.None));
    }

    [Fact]
    public async Task StartSession_WhenInspectorDoesNotExist_ThrowsInspectorNotFoundException()
    {
        var inspectorId = InspectorId.New();
        _inspectorTrackingMock.Setup(x => x.InspectorExistsAsync(inspectorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new StartSessionHandler(_sessionRepoMock.Object, _inspectorTrackingMock.Object);

        await Assert.ThrowsAsync<InspectorNotFoundException>(() =>
            handler.Handle(new StartSessionCommand(inspectorId.Value), CancellationToken.None));
    }

    [Fact]
    public async Task CloseSession_WhenSessionAlreadyClosed_ThrowsSessionAlreadyClosedException()
    {
        var session = Session.Create(InspectorId.New(), DateTime.UtcNow.AddHours(-2));
        session.Close(DateTime.UtcNow.AddHours(-1));

        _sessionRepoMock.Setup(x => x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new CloseSessionHandler(_sessionRepoMock.Object);

        await Assert.ThrowsAsync<SessionAlreadyClosedException>(() =>
            handler.Handle(new CloseSessionCommand(session.Id.Value), CancellationToken.None));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    [InlineData("ZA_DLUGA_REJESTRACJA_123")]
    [InlineData("WA@123!")]
    public async Task StartInspection_WithInvalidRegistrationNumber_ThrowsInvalidRegistrationNumberException(string invalidPlates)
    {
        var inspectorId = InspectorId.New();
        var session = Session.Create(inspectorId, DateTime.UtcNow);
        _sessionRepoMock.Setup(x => x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _inspectorTrackingMock.Setup(x => x.GetAssignedZoneAsync(inspectorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ZoneId?)null);

        var handler = new StartInspectionHandler(
            _inspectionRepoMock.Object,
            _sessionRepoMock.Object,
            _mediatorMock.Object,
            _noticesModuleMock.Object,
            _inspectorTrackingMock.Object);

        var cmd = new StartInspectionCommand(session.Id.Value, invalidPlates, 52.0, 21.0, Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidRegistrationNumberException>(() =>
            handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task StartInspection_WhenInspectorIsAssignedToAnotherZone_ThrowsInspectorNotAssignedToZoneException()
    {
        var inspectorId = InspectorId.New();
        var assignedZoneId = ZoneId.New();
        var differentZoneId = ZoneId.New();
        var session = Session.Create(inspectorId, DateTime.UtcNow);

        _sessionRepoMock.Setup(x => x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _inspectorTrackingMock.Setup(x => x.GetAssignedZoneAsync(inspectorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignedZoneId);

        var handler = new StartInspectionHandler(
            _inspectionRepoMock.Object,
            _sessionRepoMock.Object,
            _mediatorMock.Object,
            _noticesModuleMock.Object,
            _inspectorTrackingMock.Object);

        var cmd = new StartInspectionCommand(session.Id.Value, "WA12345", 52.0, 21.0, differentZoneId.Value, Guid.NewGuid());

        await Assert.ThrowsAsync<InspectorNotAssignedToZoneException>(() =>
            handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task StartInspection_WhenInspectorHasNoAssignedZone_CanInspectAnyZone()
    {
        var inspectorId = InspectorId.New();
        var anyZoneId = ZoneId.New();
        var session = Session.Create(inspectorId, DateTime.UtcNow);

        _sessionRepoMock.Setup(x => x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _inspectorTrackingMock.Setup(x => x.GetAssignedZoneAsync(inspectorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ZoneId?)null);
        _noticesModuleMock.Setup(x => x.WasNoticeIssuedForDateAsync(It.IsAny<RegistrationNumber>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new StartInspectionHandler(
            _inspectionRepoMock.Object,
            _sessionRepoMock.Object,
            _mediatorMock.Object,
            _noticesModuleMock.Object,
            _inspectorTrackingMock.Object);

        var cmd = new StartInspectionCommand(session.Id.Value, "WA12345", 52.0, 21.0, anyZoneId.Value, Guid.NewGuid());

        var inspectionId = await handler.Handle(cmd, CancellationToken.None);

        inspectionId.Should().NotBeEmpty();
        _inspectionRepoMock.Verify(r => r.AddAsync(It.IsAny<Inspection>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartInspection_WhenInspectorInspectsTheirAssignedZone_Succeeds()
    {
        var inspectorId = InspectorId.New();
        var assignedZoneId = ZoneId.New();
        var session = Session.Create(inspectorId, DateTime.UtcNow);

        _sessionRepoMock.Setup(x => x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _inspectorTrackingMock.Setup(x => x.GetAssignedZoneAsync(inspectorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignedZoneId);
        _noticesModuleMock.Setup(x => x.WasNoticeIssuedForDateAsync(It.IsAny<RegistrationNumber>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new StartInspectionHandler(
            _inspectionRepoMock.Object,
            _sessionRepoMock.Object,
            _mediatorMock.Object,
            _noticesModuleMock.Object,
            _inspectorTrackingMock.Object);

        var cmd = new StartInspectionCommand(session.Id.Value, "WA12345", 52.0, 21.0, assignedZoneId.Value, Guid.NewGuid());

        var inspectionId = await handler.Handle(cmd, CancellationToken.None);

        inspectionId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task StartInspection_WhenVehicleAlreadyHasNoticeToday_ThrowsNoticeAlreadyIssuedTodayException()
    {
        var inspectorId = InspectorId.New();
        var session = Session.Create(inspectorId, DateTime.UtcNow);

        _sessionRepoMock.Setup(x => x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _inspectorTrackingMock.Setup(x => x.GetAssignedZoneAsync(inspectorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ZoneId?)null);
        _noticesModuleMock.Setup(x => x.WasNoticeIssuedForDateAsync(new RegistrationNumber("WA12345"), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new StartInspectionHandler(
            _inspectionRepoMock.Object,
            _sessionRepoMock.Object,
            _mediatorMock.Object,
            _noticesModuleMock.Object,
            _inspectorTrackingMock.Object);

        var cmd = new StartInspectionCommand(session.Id.Value, "WA12345", 52.0, 21.0, Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<NoticeAlreadyIssuedTodayException>(() =>
            handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task StartInspection_WhenVehicleIsAwaitingSecondCheckToday_ResumesExistingInspectionInsteadOfCreatingNew()
    {
        var inspectorId = InspectorId.New();
        var session = Session.Create(inspectorId, DateTime.UtcNow);
        var existingInspection = Inspection.Create(
            session.Id, inspectorId, new RegistrationNumber("WA12345"), ZoneId.New(), StreetId.New(), 52.0, 21.0, DateTime.UtcNow.AddMinutes(-10));
        existingInspection.RecordTicketCheck(TicketCheckResult.Invalid("Brak biletu")); // -> AwaitingSecondCheck

        _sessionRepoMock.Setup(x => x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _inspectorTrackingMock.Setup(x => x.GetAssignedZoneAsync(inspectorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ZoneId?)null);
        _noticesModuleMock.Setup(x => x.WasNoticeIssuedForDateAsync(It.IsAny<RegistrationNumber>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _inspectionRepoMock.Setup(x => x.GetInspectionAwaitingForSecondCheckAsync(new RegistrationNumber("WA12345"), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInspection);

        var handler = new StartInspectionHandler(
            _inspectionRepoMock.Object,
            _sessionRepoMock.Object,
            _mediatorMock.Object,
            _noticesModuleMock.Object,
            _inspectorTrackingMock.Object);

        var returnedId = await handler.Handle(
            new StartInspectionCommand(session.Id.Value, "WA12345", 52.0, 21.0, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        returnedId.Should().Be(existingInspection.Id.Value);
        _inspectionRepoMock.Verify(r => r.AddAsync(It.IsAny<Inspection>(), It.IsAny<CancellationToken>()), Times.Never);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<InspectionStartedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddViolationHandler_WhenAttemptingToAddNoTicketManually_ThrowsCannotAddNoTicketManuallyException()
    {
        var inspection = Inspection.Create(
            SessionId.New(), InspectorId.New(), new RegistrationNumber("WA12345"), ZoneId.New(), StreetId.New(), 52.0, 21.0, DateTime.UtcNow);

        _inspectionRepoMock.Setup(r => r.GetByIdAsync(inspection.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inspection);
        _violationsModuleMock.Setup(v => v.ViolationTypeExistsAsync(ViolationTypeId.NoTicket, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new AddViolationHandler(_inspectionRepoMock.Object, _violationsModuleMock.Object);

        await Assert.ThrowsAsync<CannotAddNoTicketManuallyException>(() =>
            handler.Handle(new AddViolationCommand(inspection.Id.Value, ViolationTypeId.NoTicket.Value), CancellationToken.None));
    }

    [Fact]
    public async Task IssueNoticeHandler_WhenInspectionIsApprovedOrAwaitingDecision_ThrowsInvalidInspectionStateException()
    {
        var awaitingInspection = Inspection.Create(
            SessionId.New(), InspectorId.New(), new RegistrationNumber("WA12345"), ZoneId.New(), StreetId.New(), 52.0, 21.0, DateTime.UtcNow);

        var approvedInspection = Inspection.Create(
            SessionId.New(), InspectorId.New(), new RegistrationNumber("WA12345"), ZoneId.New(), StreetId.New(), 52.0, 21.0, DateTime.UtcNow);
        approvedInspection.Approve();

        _inspectionRepoMock.Setup(r => r.GetByIdAsync(awaitingInspection.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(awaitingInspection);
        _inspectionRepoMock.Setup(r => r.GetByIdAsync(approvedInspection.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(approvedInspection);

        var handler = new IssueNoticeHandler(_inspectionRepoMock.Object, _violationsModuleMock.Object, _mediatorMock.Object);

        await Assert.ThrowsAsync<InvalidInspectionStateException>(() =>
            handler.Handle(new IssueNoticeCommand(awaitingInspection.Id.Value), CancellationToken.None));

        await Assert.ThrowsAsync<InvalidInspectionStateException>(() =>
            handler.Handle(new IssueNoticeCommand(approvedInspection.Id.Value), CancellationToken.None));
    }

    [Fact]
    public async Task IssueNoticeHandler_WhenViolationFoundButNoPhotosAttached_ThrowsMissingPhotosForNoticeException()
    {
        var inspection = Inspection.Create(
            SessionId.New(), InspectorId.New(), new RegistrationNumber("WA12345"), ZoneId.New(), StreetId.New(), 52.0, 21.0, DateTime.UtcNow);
        inspection.AddVisualViolation(ViolationTypeId.New()); // -> ViolationFound (brak zdjęć)

        _inspectionRepoMock.Setup(r => r.GetByIdAsync(inspection.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inspection);

        var handler = new IssueNoticeHandler(_inspectionRepoMock.Object, _violationsModuleMock.Object, _mediatorMock.Object);

        await Assert.ThrowsAsync<MissingPhotosForNoticeException>(() =>
            handler.Handle(new IssueNoticeCommand(inspection.Id.Value), CancellationToken.None));
    }
}

