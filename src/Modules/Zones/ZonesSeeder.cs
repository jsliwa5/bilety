namespace PTickets.Modules.Zones;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Zones.Contracts.Events;
using PTickets.Modules.Zones.Data;

public static class ZonesSeeder
{
    public static async Task SeedAsync(ZonesDbContext db, IMediator mediator, CancellationToken ct = default)
    {
        if (await db.Zones.AnyAsync(ct))
        {
            return;
        }

        var scheduleStandardMonFri = new PaidParkingSchedule(
            new TimeOnly(8, 0),
            new TimeOnly(18, 0),
            [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday]
        );

        var scheduleAllWeek = new PaidParkingSchedule(
            new TimeOnly(0, 0),
            new TimeOnly(23, 59),
            [
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday,
                DayOfWeek.Sunday
            ]
        );

        // 1. Strefa A - Centrum (MultiStreet)
        var centrum = Zone.CreateMultiStreet("Strefa A (Centrum)", scheduleStandardMonFri);
        centrum.AddStreet("Marszałkowska");
        centrum.AddStreet("Świętokrzyska");
        centrum.AddStreet("Nowy Świat");
        centrum.AddStreet("Chmielna");

        // 2. Strefa B - Mokotów (MultiStreet)
        var mokotow = Zone.CreateMultiStreet("Strefa B (Mokotów)", scheduleStandardMonFri);
        mokotow.AddStreet("Puławska");
        mokotow.AddStreet("Domaniewska");
        mokotow.AddStreet("Wołoska");

        // 3. Strefa C - Wola (MultiStreet)
        var wola = Zone.CreateMultiStreet("Strefa C (Wola)", scheduleStandardMonFri);
        wola.AddStreet("Towarowa");
        wola.AddStreet("Prosta");

        // 4. Strefa D - Stare Miasto (Single / Standalone - plac/parking)
        var stareMiasto = Zone.CreateSingle("Strefa D (Stare Miasto)", scheduleAllWeek);

        var zones = new[] { centrum, mokotow, wola, stareMiasto };

        await db.Zones.AddRangeAsync(zones, ct);
        await db.SaveChangesAsync(ct);

        foreach (var zone in zones)
        {
            await mediator.Publish(new ZoneCreatedEvent(zone.Id, zone.Name), ct);

            foreach (var street in zone.Streets)
            {
                await mediator.Publish(new StreetCreatedEvent(street.Id, zone.Id, street.Name), ct);
            }
        }
    }
}

