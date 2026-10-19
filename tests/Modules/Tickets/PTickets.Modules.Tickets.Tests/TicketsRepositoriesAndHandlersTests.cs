namespace PTickets.Modules.Tickets.Tests;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Tickets.Application.Commands;
using PTickets.Modules.Tickets.Application.EventHandlers;
using PTickets.Modules.Tickets.Domain;
using PTickets.Modules.Tickets.Infrastructure.Persistence;
using PTickets.Modules.Zones.Contracts.Events;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;
using Xunit;

public class TicketsRepositoriesAndHandlersTests
{
    private static TicketsDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TicketsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TicketsDbContext(options);
    }

    [Fact]
    public async Task StreetCreatedEventHandler_WhenDuplicateEventReceived_IsIdempotentAndDoesNotDuplicateMapping()
    {
        await using var db = CreateInMemoryDbContext();
        var repo = new EfStreetZoneMappingRepository(db);
        var handler = new StreetCreatedEventHandler(repo);

        var streetId = StreetId.New();
        var zoneId = ZoneId.New();
        var evt = new StreetCreatedEvent(streetId, zoneId, "Marszałkowska");

        // Wywołanie 2 razy tego samego zdarzenia
        await handler.Handle(evt, CancellationToken.None);
        await handler.Handle(evt, CancellationToken.None);

        var mappings = await db.StreetZoneMappings.ToListAsync();
        mappings.Should().ContainSingle(m => m.StreetId == streetId && m.ZoneId == zoneId);
    }

    [Fact]
    public async Task EfTicketRepository_ExistsByExternalIdAsync_DetectsDuplicatesFromRabbitMq()
    {
        await using var db = CreateInMemoryDbContext();
        var repo = new EfTicketRepository(db);

        var now = DateTime.UtcNow;
        var ticket = Ticket.CreateFromExternal("rabbit-msg-001", new RegistrationNumber("WA12345"), now, now.AddHours(1), "SkyCash", "ZoneA");

        await repo.AddAsync(ticket);
        await repo.SaveChangesAsync();

        (await repo.ExistsByExternalIdAsync("rabbit-msg-001")).Should().BeTrue();
        (await repo.ExistsByExternalIdAsync("rabbit-msg-999")).Should().BeFalse();
    }

    [Fact]
    public async Task EfResidentCardRepository_WhenCardIsIssuedForStreetA_DoesNotApplyToStreetB()
    {
        await using var db = CreateInMemoryDbContext();
        var repo = new EfResidentCardRepository(db);
        var handler = new IssueResidentCardHandler(repo);

        var reg = new RegistrationNumber("WA12345");
        var streetA = StreetId.New();
        var streetB = StreetId.New();
        var now = DateTime.UtcNow;

        await handler.Handle(new IssueResidentCardCommand(reg, streetA, now.AddDays(-5), now.AddDays(30)), CancellationToken.None);

        var foundOnStreetA = await repo.FindActiveCardAsync(reg, streetA, now);
        var foundOnStreetB = await repo.FindActiveCardAsync(reg, streetB, now);

        foundOnStreetA.Should().NotBeNull();
        foundOnStreetB.Should().BeNull();
    }

    [Fact]
    public async Task EfTicketRepository_WhenTicketIsExpired_DoesNotReturnItAsActive()
    {
        await using var db = CreateInMemoryDbContext();
        var repo = new EfTicketRepository(db);

        var reg = new RegistrationNumber("WA12345");
        var streetId = StreetId.New();
        var now = DateTime.UtcNow;

        // Bilet wygasł minutę temu
        var expiredTicket = Ticket.Create(reg, streetId, now.AddHours(-2), now.AddMinutes(-1), "mPay");

        await repo.AddAsync(expiredTicket);
        await repo.SaveChangesAsync();

        var found = await repo.FindActiveTicketAsync(reg, streetId, now);
        found.Should().BeNull();
    }
}

