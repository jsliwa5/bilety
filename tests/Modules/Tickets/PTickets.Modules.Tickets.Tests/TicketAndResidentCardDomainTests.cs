namespace PTickets.Modules.Tickets.Tests;

using FluentAssertions;
using PTickets.Modules.Tickets.Domain;
using PTickets.Modules.Tickets.Domain.Exceptions;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;
using Xunit;

public class TicketAndResidentCardDomainTests
{
    private static readonly DateTime Now = new(2026, 10, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void TicketCreate_WithValidParameters_SucceedsAndChecksBoundariesCorrectly()
    {
        var reg = new RegistrationNumber("WA12345");
        var streetId = StreetId.New();
        var validFrom = Now;
        var validTo = Now.AddHours(2);

        var ticket = Ticket.Create(reg, streetId, validFrom, validTo, "  SkyCash  ");

        ticket.RegistrationNumber.Should().Be(reg);
        ticket.StreetId.Should().Be(streetId);
        ticket.ProviderName.Should().Be("SkyCash");

        // Granice czasowe
        ticket.IsValidAt(validFrom).Should().BeTrue();
        ticket.IsValidAt(validTo).Should().BeTrue();
        ticket.IsValidAt(validFrom.AddHours(1)).Should().BeTrue();
        ticket.IsValidAt(validFrom.AddSeconds(-1)).Should().BeFalse();
        ticket.IsValidAt(validTo.AddSeconds(1)).Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void TicketCreate_WhenValidToIsEqualOrBeforeValidFrom_ThrowsInvalidTicketDatesException(int minutesOffset)
    {
        var reg = new RegistrationNumber("WA12345");

        Assert.Throws<InvalidTicketDatesException>(() =>
            Ticket.Create(reg, StreetId.New(), Now, Now.AddMinutes(minutesOffset), "SkyCash"));

        Assert.Throws<InvalidTicketDatesException>(() =>
            Ticket.CreateFromExternal("ext-1", reg, Now, Now.AddMinutes(minutesOffset), "SkyCash", "ZoneA"));
    }

    [Fact]
    public void TicketCreate_WhenStreetIdIsEmpty_ThrowsInvalidTicketDataException()
    {
        var reg = new RegistrationNumber("WA12345");

        Assert.Throws<InvalidTicketDataException>(() =>
            Ticket.Create(reg, StreetId.Empty, Now, Now.AddHours(1), "SkyCash"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void TicketCreateFromExternal_WhenExternalIdOrProviderIsEmpty_ThrowsInvalidTicketDataException(string emptyValue)
    {
        var reg = new RegistrationNumber("WA12345");

        Assert.Throws<InvalidTicketDataException>(() =>
            Ticket.CreateFromExternal(emptyValue, reg, Now, Now.AddHours(1), "SkyCash"));

        Assert.Throws<InvalidTicketDataException>(() =>
            Ticket.CreateFromExternal("ext-123", reg, Now, Now.AddHours(1), emptyValue));
    }

    [Fact]
    public void ResidentCardCreate_WhenValidDates_Succeeds()
    {
        var reg = new RegistrationNumber("WA12345");
        var streetId = StreetId.New();

        var card = ResidentCard.Create(reg, streetId, Now, Now.AddDays(365));

        card.RegistrationNumber.Should().Be(reg);
        card.StreetId.Should().Be(streetId);
        card.IsValidAt(Now.AddDays(100)).Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void ResidentCardCreate_WhenValidToIsEqualOrBeforeValidFrom_ThrowsInvalidResidentCardDatesException(int minutesOffset)
    {
        var reg = new RegistrationNumber("WA12345");

        Assert.Throws<InvalidResidentCardDatesException>(() =>
            ResidentCard.Create(reg, StreetId.New(), Now, Now.AddMinutes(minutesOffset)));
    }

    [Fact]
    public void ResidentCardCreate_WhenStreetIdIsEmpty_ThrowsInvalidTicketDataException()
    {
        var reg = new RegistrationNumber("WA12345");

        Assert.Throws<InvalidTicketDataException>(() =>
            ResidentCard.Create(reg, StreetId.Empty, Now, Now.AddDays(30)));
    }
}

