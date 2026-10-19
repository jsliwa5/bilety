namespace PTickets.Modules.Notices.Tests;

using FluentAssertions;
using PTickets.Modules.Notices.Common.Data;
using PTickets.Modules.Notices.Common.Exceptions;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public class NoticeDomainTests
{
    private static Notice CreateSampleNotice(decimal penalty = 150m, decimal surcharge = 50m)
        => Notice.Create(
            NoticeId.New(),
            InspectionId.New(),
            new RegistrationNumber("WA12345"),
            penalty,
            surcharge,
            new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void Create_WithValidAmounts_InitializesIssuedNoticeAndCalculatesTotalAmount()
    {
        var notice = CreateSampleNotice(150m, 30m);

        notice.Status.Should().Be(NoticeStatus.Issued);
        notice.PenaltyAmount.Should().Be(150m);
        notice.Surcharge.Should().Be(30m);
        notice.TotalAmount.Should().Be(180m);
    }

    [Fact]
    public void Create_WithZeroSurcharge_CalculatesTotalAmountCorrectly()
    {
        var notice = CreateSampleNotice(150m, 0m);

        notice.TotalAmount.Should().Be(150m);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-150, 0)]
    [InlineData(150, -0.01)]
    public void Create_WithInvalidPenaltyOrNegativeSurcharge_ThrowsInvalidNoticeAmountException(decimal penalty, decimal surcharge)
    {
        Action act = () => CreateSampleNotice(penalty, surcharge);

        act.Should().Throw<InvalidNoticeAmountException>();
    }

    [Fact]
    public void MarkAsPaid_WhenIssued_ChangesStatusToPaid()
    {
        var notice = CreateSampleNotice();

        notice.MarkAsPaid();

        notice.Status.Should().Be(NoticeStatus.Paid);
    }

    [Fact]
    public void MarkAsPaid_WhenSentToDebtCollection_ChangesStatusToPaid()
    {
        var notice = CreateSampleNotice();
        notice.SendToDebtCollection();

        notice.MarkAsPaid();

        notice.Status.Should().Be(NoticeStatus.Paid);
    }

    [Fact]
    public void AppealLifecycle_WhenAppealedAndRejected_ReturnsToIssuedAndCanBePaid()
    {
        var notice = CreateSampleNotice();

        // 1. Kierowca składa odwołanie
        notice.MarkAsAppealed();
        notice.Status.Should().Be(NoticeStatus.Appealed);

        // 2. Urzędnik odrzuca odwołanie -> wraca do Issued
        notice.RejectAppeal();
        notice.Status.Should().Be(NoticeStatus.Issued);

        // 3. Kierowca opłaca wezwanie
        notice.MarkAsPaid();
        notice.Status.Should().Be(NoticeStatus.Paid);
    }

    [Fact]
    public void AppealLifecycle_WhenAppealedAndApproved_ChangesStatusToCancelled()
    {
        var notice = CreateSampleNotice();

        notice.MarkAsAppealed();
        notice.Cancel();

        notice.Status.Should().Be(NoticeStatus.Cancelled);
    }

    [Fact]
    public void MarkAsPaid_WhenAlreadyPaidOrCancelledOrAppealed_ThrowsCannotPayNoticeException()
    {
        var paidNotice = CreateSampleNotice();
        paidNotice.MarkAsPaid();

        var cancelledNotice = CreateSampleNotice();
        cancelledNotice.Cancel();

        var appealedNotice = CreateSampleNotice();
        appealedNotice.MarkAsAppealed();

        Assert.Throws<CannotPayNoticeException>(() => paidNotice.MarkAsPaid());
        Assert.Throws<CannotPayNoticeException>(() => cancelledNotice.MarkAsPaid());
        Assert.Throws<CannotPayNoticeException>(() => appealedNotice.MarkAsPaid());
    }

    [Fact]
    public void Cancel_WhenAlreadyPaidOrInDebtCollection_ThrowsCannotCancelNoticeException()
    {
        var paidNotice = CreateSampleNotice();
        paidNotice.MarkAsPaid();

        var debtNotice = CreateSampleNotice();
        debtNotice.SendToDebtCollection();

        Assert.Throws<CannotCancelNoticeException>(() => paidNotice.Cancel());
        Assert.Throws<CannotCancelNoticeException>(() => debtNotice.Cancel());
    }

    [Fact]
    public void SendToDebtCollection_WhenAppealedOrPaidOrCancelled_ThrowsInvalidNoticeStatusTransitionException()
    {
        var appealedNotice = CreateSampleNotice();
        appealedNotice.MarkAsAppealed();

        var paidNotice = CreateSampleNotice();
        paidNotice.MarkAsPaid();

        var cancelledNotice = CreateSampleNotice();
        cancelledNotice.Cancel();

        // Nie wolno wysłać do windykacji w trakcie rozpatrywania reklamacji!
        Assert.Throws<InvalidNoticeStatusTransitionException>(() => appealedNotice.SendToDebtCollection());
        Assert.Throws<InvalidNoticeStatusTransitionException>(() => paidNotice.SendToDebtCollection());
        Assert.Throws<InvalidNoticeStatusTransitionException>(() => cancelledNotice.SendToDebtCollection());
    }

    [Fact]
    public void MarkAsAppealed_WhenNotIssued_ThrowsInvalidNoticeStatusTransitionException()
    {
        var paidNotice = CreateSampleNotice();
        paidNotice.MarkAsPaid();

        var debtNotice = CreateSampleNotice();
        debtNotice.SendToDebtCollection();

        Assert.Throws<InvalidNoticeStatusTransitionException>(() => paidNotice.MarkAsAppealed());
        Assert.Throws<InvalidNoticeStatusTransitionException>(() => debtNotice.MarkAsAppealed());
    }

    [Fact]
    public void RejectAppeal_WhenStatusIsNotAppealed_ThrowsInvalidNoticeStatusTransitionException()
    {
        var issuedNotice = CreateSampleNotice();

        Assert.Throws<InvalidNoticeStatusTransitionException>(() => issuedNotice.RejectAppeal());
    }
}

