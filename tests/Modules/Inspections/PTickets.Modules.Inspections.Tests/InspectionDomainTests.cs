namespace PTickets.Modules.Inspections.Tests;

using FluentAssertions;
using PTickets.Modules.Inspections.Domain;
using PTickets.Modules.Inspections.Domain.Exceptions;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;
using Xunit;

public class InspectionDomainTests
{
    private static Inspection CreateNewInspection()
        => Inspection.Create(
            SessionId.New(),
            InspectorId.New(),
            new RegistrationNumber("WA12345"),
            ZoneId.New(),
            StreetId.New(),
            52.2297,
            21.0122,
            DateTime.UtcNow);

    [Fact]
    public void HappyPath_FirstTicketCheckValid_ImmediatelyApprovesInspection()
    {
        var inspection = CreateNewInspection();
        var validResult = TicketCheckResult.Valid(DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddMinutes(50), "MobileApp");

        inspection.RecordTicketCheck(validResult);

        inspection.Status.Should().Be(InspectionStatus.Approved);
        inspection.Violations.Should().BeEmpty();
        inspection.TimeOfFirstCheck.Should().NotBeNull();
    }

    [Fact]
    public void HappyPath_SecondTicketCheckValidInGracePeriod_ApprovesInspectionWithoutViolation()
    {
        var inspection = CreateNewInspection();

        // 1. Pierwsze sprawdzenie: brak biletu -> AwaitingSecondCheck
        inspection.RecordTicketCheck(TicketCheckResult.Invalid("Brak biletu"));
        inspection.Status.Should().Be(InspectionStatus.AwaitingSecondCheck);

        // 2. Dodanie zdjęcia dokumentującego 1. kontrolę
        inspection.AttachPhotos([FileId.New()]);
        inspection.Status.Should().Be(InspectionStatus.AwaitingSecondCheck);

        // 3. Drugie sprawdzenie: kierowca kupił bilet w okresie karencji -> Approved
        inspection.RecordTicketCheck(TicketCheckResult.Valid(DateTime.UtcNow.AddMinutes(-2), DateTime.UtcNow.AddMinutes(30), "SkyCash"));

        inspection.Status.Should().Be(InspectionStatus.Approved);
        inspection.Violations.Should().BeEmpty();
        inspection.TimeOfSecondCheck.Should().NotBeNull();
    }

    [Fact]
    public void HappyPath_BothTicketChecksInvalidAndPhotosAttached_AllowsIssuingNotice()
    {
        var inspection = CreateNewInspection();

        // 1. Pierwsze sprawdzenie: brak biletu
        inspection.RecordTicketCheck(TicketCheckResult.Invalid("Brak biletu"));
        inspection.Status.Should().Be(InspectionStatus.AwaitingSecondCheck);

        // 2. Zdjęcie z 1. sprawdzenia
        inspection.AttachPhotos([FileId.New()]);

        // 3. Drugie sprawdzenie: nadal brak biletu -> ViolationFound + dodane wykroczenie NoTicket
        inspection.RecordTicketCheck(TicketCheckResult.Invalid("Brak biletu"));
        inspection.Status.Should().Be(InspectionStatus.ViolationFound);
        inspection.Violations.Should().ContainSingle(v =>
            v.ViolationTypeId == ViolationTypeId.NoTicket &&
            v.Source == ViolationSource.TicketCheck);

        // 4. Zdjęcie z 2. sprawdzenia -> PhotosAttached
        inspection.AttachPhotos([FileId.New()]);
        inspection.Status.Should().Be(InspectionStatus.PhotosAttached);

        // 5. Wystawienie zawiadomienia -> NoticeIssued
        var noticeId = NoticeId.New();
        inspection.MarkNoticeIssued(noticeId);
        inspection.Status.Should().Be(InspectionStatus.NoticeIssued);
        inspection.NoticeId.Should().Be(noticeId);
    }

    [Fact]
    public void AddVisualViolation_WhenAttemptingToAddNoTicketManually_ThrowsCannotAddNoTicketManuallyException()
    {
        var inspection = CreateNewInspection();

        Assert.Throws<CannotAddNoTicketManuallyException>(() =>
            inspection.AddVisualViolation(ViolationTypeId.NoTicket));
    }

    [Fact]
    public void AttachPhotos_BeforeFindingViolationOrAwaitingSecondCheck_ThrowsInvalidInspectionStateException()
    {
        var inspection = CreateNewInspection(); // Status = AwaitingDecision

        Assert.Throws<InvalidInspectionStateException>(() =>
            inspection.AttachPhotos([FileId.New()]));
    }

    [Fact]
    public void AttachPhotos_WhenPhotoListIsEmpty_ThrowsNoPhotosProvidedException()
    {
        var inspection = CreateNewInspection();
        inspection.AddVisualViolation(ViolationTypeId.New());

        Assert.Throws<NoPhotosProvidedException>(() =>
            inspection.AttachPhotos([]));
    }

    [Fact]
    public void SecondTicketCheck_WhenFirstCheckPhotosAreMissing_ThrowsMissingFirstCheckPhotosException()
    {
        var inspection = CreateNewInspection();
        inspection.RecordTicketCheck(TicketCheckResult.Invalid("Brak biletu")); // -> AwaitingSecondCheck

        // Próba 2. sprawdzenia bez dodania zdjęcia z 1. kontroli
        Assert.Throws<MissingFirstCheckPhotosException>(() =>
            inspection.RecordTicketCheck(TicketCheckResult.Invalid("Brak biletu")));
    }

    [Fact]
    public void RecordTicketCheck_WhenVisualViolationAlreadyFound_ThrowsInvalidInspectionStateException()
    {
        var inspection = CreateNewInspection();
        inspection.AddVisualViolation(ViolationTypeId.New()); // -> ViolationFound

        Assert.Throws<InvalidInspectionStateException>(() =>
            inspection.RecordTicketCheck(TicketCheckResult.Valid(DateTime.UtcNow, DateTime.UtcNow.AddHours(1))));
    }

    [Fact]
    public void AddVisualViolation_AfterNoTicketViolationAlreadyFound_ThrowsInvalidInspectionStateException()
    {
        var inspection = CreateNewInspection();
        inspection.RecordTicketCheck(TicketCheckResult.Invalid("Brak biletu"));
        inspection.AttachPhotos([FileId.New()]);
        inspection.RecordTicketCheck(TicketCheckResult.Invalid("Brak biletu")); // -> ViolationFound

        Assert.Throws<InvalidInspectionStateException>(() =>
            inspection.AddVisualViolation(ViolationTypeId.New()));
    }

    [Fact]
    public void MarkNoticeIssued_WhenInspectionIsApproved_ThrowsInvalidInspectionStateException()
    {
        var inspection = CreateNewInspection();
        inspection.Approve();

        Assert.Throws<InvalidInspectionStateException>(() =>
            inspection.MarkNoticeIssued(NoticeId.New()));
    }

    [Fact]
    public void MarkNoticeIssued_WhenInspectionIsAwaitingDecision_ThrowsInvalidInspectionStateException()
    {
        var inspection = CreateNewInspection(); // AwaitingDecision

        Assert.Throws<InvalidInspectionStateException>(() =>
            inspection.MarkNoticeIssued(NoticeId.New()));
    }

    [Fact]
    public void MarkNoticeIssued_WhenPhotosNotAttachedYet_ThrowsMissingPhotosForNoticeException()
    {
        var inspection = CreateNewInspection();
        inspection.AddVisualViolation(ViolationTypeId.New()); // Status = ViolationFound (brak zdjęć)

        Assert.Throws<MissingPhotosForNoticeException>(() =>
            inspection.MarkNoticeIssued(NoticeId.New()));
    }

    [Fact]
    public void Approve_WhenViolationExists_ThrowsCannotApproveInspectionWithViolationsException()
    {
        var inspection = CreateNewInspection();
        inspection.AddVisualViolation(ViolationTypeId.New());

        Assert.Throws<CannotApproveInspectionWithViolationsException>(() =>
            inspection.Approve());
    }
}

