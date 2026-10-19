namespace PTickets.Modules.Tickets.Tests;

using FluentAssertions;
using MediatR;
using Moq;
using PTickets.Modules.Tickets.Application.Commands;
using PTickets.Modules.Tickets.Application.Services;
using PTickets.Modules.Tickets.Domain;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;
using Xunit;

public class TicketVerificationServiceTests
{
    private readonly Mock<IStreetZoneMappingRepository> _mappingRepoMock = new();
    private readonly Mock<IResidentCardRepository> _residentCardRepoMock = new();
    private readonly Mock<ITicketRepository> _ticketRepoMock = new();
    private readonly Mock<ISender> _senderMock = new();

    private static readonly DateTime CheckTime = new(2026, 10, 19, 12, 0, 0, DateTimeKind.Utc);
    private static readonly RegistrationNumber Reg = new("WA12345");
    private static readonly StreetId StreetA1 = StreetId.New();
    private static readonly ZoneId ZoneA = ZoneId.New();

    [Fact]
    public async Task VerifyTicket_Step1_WhenActiveResidentCardExistsForStreet_ReturnsValidImmediatelyWithoutQueryingTicketsOrProviders()
    {
        // Arrange
        var card = ResidentCard.Create(Reg, StreetA1, CheckTime.AddDays(-10), CheckTime.AddDays(10));
        _residentCardRepoMock.Setup(r => r.FindActiveCardAsync(Reg, StreetA1, CheckTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        var providerMock = new Mock<ITicketProvider>();

        var service = new TicketVerificationService(
            _mappingRepoMock.Object,
            _residentCardRepoMock.Object,
            _ticketRepoMock.Object,
            [providerMock.Object],
            _senderMock.Object);

        // Act
        var result = await service.VerifyTicketAsync(Reg, StreetA1, CheckTime);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ProviderMessage.Should().Be("Karta Mieszkańca");

        // Upewniamy się, że dalsze kroki nie zostały nawet wywołane (Fast-Path)
        _ticketRepoMock.Verify(r => r.FindActiveTicketAsync(It.IsAny<RegistrationNumber>(), It.IsAny<StreetId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        providerMock.Verify(p => p.CheckAsync(It.IsAny<RegistrationNumber>(), It.IsAny<StreetId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VerifyTicket_Step2_WhenLocalTicketForExactStreetExists_ReturnsValidWithoutQueryingProviders()
    {
        // Arrange
        var localTicket = Ticket.Create(Reg, StreetA1, CheckTime.AddMinutes(-30), CheckTime.AddMinutes(30), "LocalProvider");
        _ticketRepoMock.Setup(r => r.FindActiveTicketAsync(Reg, StreetA1, CheckTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync(localTicket);

        var providerMock = new Mock<ITicketProvider>();

        var service = new TicketVerificationService(
            _mappingRepoMock.Object,
            _residentCardRepoMock.Object,
            _ticketRepoMock.Object,
            [providerMock.Object],
            _senderMock.Object);

        // Act
        var result = await service.VerifyTicketAsync(Reg, StreetA1, CheckTime);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ProviderMessage.Should().Be("LocalProvider");

        providerMock.Verify(p => p.CheckAsync(It.IsAny<RegistrationNumber>(), It.IsAny<StreetId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VerifyTicket_Step3_WhenRabbitMqTicketHasParkingZoneMatchingMappedZoneId_ReturnsValid()
    {
        // Arrange — Ulica StreetA1 należy do strefy ZoneA
        _mappingRepoMock.Setup(m => m.GetByStreetIdAsync(StreetA1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(StreetZoneMapping.Create(StreetA1, ZoneA));

        // Bilet z RabbitMQ ma ParkingZone = ZoneA.Value.ToString() (np. małymi literami)
        var rabbitTicket = Ticket.CreateFromExternal(
            "tkt-100",
            Reg,
            CheckTime.AddMinutes(-15),
            CheckTime.AddMinutes(45),
            "mPay",
            ZoneA.Value.ToString().ToLowerInvariant());

        _ticketRepoMock.Setup(r => r.FindActiveTicketsByRegistrationAsync(Reg, CheckTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync([rabbitTicket]);

        var service = new TicketVerificationService(
            _mappingRepoMock.Object,
            _residentCardRepoMock.Object,
            _ticketRepoMock.Object,
            [],
            _senderMock.Object);

        // Act
        var result = await service.VerifyTicketAsync(Reg, StreetA1, CheckTime);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ProviderMessage.Should().Be("mPay");
    }

    [Fact]
    public async Task VerifyTicket_Step3EdgeCase_WhenRabbitMqTicketWasBoughtForDifferentZone_ReturnsInvalid()
    {
        // Arrange — Kontrolowana ulica StreetA1 należy do ZoneA
        var zoneB = ZoneId.New();
        _mappingRepoMock.Setup(m => m.GetByStreetIdAsync(StreetA1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(StreetZoneMapping.Create(StreetA1, ZoneA));

        // Kierowca ma ważny bilet z RabbitMQ, ale kupiony na ZoneB!
        var wrongZoneTicket = Ticket.CreateFromExternal(
            "tkt-wrong-zone",
            Reg,
            CheckTime.AddMinutes(-15),
            CheckTime.AddMinutes(45),
            "SkyCash",
            zoneB.Value.ToString());

        _ticketRepoMock.Setup(r => r.FindActiveTicketsByRegistrationAsync(Reg, CheckTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync([wrongZoneTicket]);

        var service = new TicketVerificationService(
            _mappingRepoMock.Object,
            _residentCardRepoMock.Object,
            _ticketRepoMock.Object,
            [],
            _senderMock.Object);

        // Act
        var result = await service.VerifyTicketAsync(Reg, StreetA1, CheckTime);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyTicket_Step3EdgeCase_WhenRabbitMqTicketHasNullParkingZone_IgnoresTicketAndReturnsInvalid()
    {
        var ticketWithoutZone = Ticket.CreateFromExternal(
            "tkt-no-zone",
            Reg,
            CheckTime.AddMinutes(-15),
            CheckTime.AddMinutes(45),
            "SkyCash",
            parkingZone: null);

        _ticketRepoMock.Setup(r => r.FindActiveTicketsByRegistrationAsync(Reg, CheckTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync([ticketWithoutZone]);

        var service = new TicketVerificationService(
            _mappingRepoMock.Object,
            _residentCardRepoMock.Object,
            _ticketRepoMock.Object,
            [],
            _senderMock.Object);

        var result = await service.VerifyTicketAsync(Reg, StreetA1, CheckTime);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyTicket_Step4_WhenFirstProviderFailsAndSecondProviderSucceeds_RecordsExternalTicketLocallyAndReturnsValid()
    {
        // Arrange
        _ticketRepoMock.Setup(r => r.FindActiveTicketsByRegistrationAsync(Reg, CheckTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var provider1Mock = new Mock<ITicketProvider>();
        provider1Mock.Setup(p => p.ProviderName).Returns("Provider1");
        provider1Mock.Setup(p => p.CheckAsync(Reg, StreetA1, CheckTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TicketCheckResult.Invalid("Brak w Provider1"));

        var validFrom = CheckTime.AddMinutes(-10);
        var validTo = CheckTime.AddMinutes(50);

        var provider2Mock = new Mock<ITicketProvider>();
        provider2Mock.Setup(p => p.ProviderName).Returns("Provider2");
        provider2Mock.Setup(p => p.CheckAsync(Reg, StreetA1, CheckTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TicketCheckResult.Valid(validFrom, validTo, "Provider2"));

        var provider3Mock = new Mock<ITicketProvider>();

        var service = new TicketVerificationService(
            _mappingRepoMock.Object,
            _residentCardRepoMock.Object,
            _ticketRepoMock.Object,
            [provider1Mock.Object, provider2Mock.Object, provider3Mock.Object],
            _senderMock.Object);

        // Act
        var result = await service.VerifyTicketAsync(Reg, StreetA1, CheckTime);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ProviderMessage.Should().Be("Provider2");

        // Sprawdzenie, czy bilet został zapisany lokalnie przez RecordExternalTicketCommand
        _senderMock.Verify(s => s.Send(
            It.Is<RecordExternalTicketCommand>(cmd =>
                cmd.RegistrationNumber == Reg &&
                cmd.StreetId == StreetA1 &&
                cmd.ValidFrom == validFrom &&
                cmd.ValidTo == validTo &&
                cmd.ProviderName == "Provider2"),
            It.IsAny<CancellationToken>()), Times.Once);

        // Trzeci dostawca nie powinien być już odpytywany
        provider3Mock.Verify(p => p.CheckAsync(It.IsAny<RegistrationNumber>(), It.IsAny<StreetId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

