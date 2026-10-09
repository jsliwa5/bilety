namespace PTickets.Modules.Zones;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Zones.Contracts.Events;
using PTickets.Modules.Zones.Common.Data;

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
        var centrum = Zone.CreateMultiStreet("Strefa A (Centrum)", scheduleStandardMonFri, Guid.Parse("A2F7ACB7-C376-4CBF-9AE9-D74DE9D1E08A"));
        centrum.AddStreet("MarszaĹ‚kowska", null, Guid.Parse("4D2EF8BF-88B8-4554-87BA-6D29B2AE4814"));
        centrum.AddStreet("ĹšwiÄ™tokrzyska", null, Guid.Parse("9B866022-BAB3-4245-92EA-4AF1964BB40B"));
        centrum.AddStreet("Nowy Ĺšwiat", null, Guid.Parse("229968C1-AF99-4373-9468-1A290B0037EF"));
        centrum.AddStreet("Chmielna", null, Guid.Parse("C7E8899D-DD77-4A1C-8556-7ABEE06F1F55"));

        // 2. Strefa B - MokotĂłw (MultiStreet)
        var mokotow = Zone.CreateMultiStreet("Strefa B (MokotĂłw)", scheduleStandardMonFri, Guid.Parse("BA4EC572-0D74-47ED-83B9-C63113449F61"));
        mokotow.AddStreet("PuĹ‚awska", null, Guid.Parse("895838BE-9EA2-465E-8BEA-3351D2318B41"));
        mokotow.AddStreet("Domaniewska", null, Guid.Parse("83D744F4-F187-4C0F-9701-13FC7A3F0A46"));
        mokotow.AddStreet("WoĹ‚oska", null, Guid.Parse("547EA554-CFE4-4F02-A17C-7E4811C3E5A8"));

        // 3. Strefa C - Wola (MultiStreet)
        var wola = Zone.CreateMultiStreet("Strefa C (Wola)", scheduleStandardMonFri, Guid.Parse("3626A1D9-1C1B-499B-97B2-ACC5F9989923"));
        wola.AddStreet("Towarowa", null, Guid.Parse("36C598F3-C4F9-44CC-BD1C-CEB6D337A8E0"));
        wola.AddStreet("Prosta", null, Guid.Parse("DFA5B564-F2AC-4BCB-8EC9-04FCFE0BE3B5"));

        // 4. Strefa D - Stare Miasto (Single / Standalone - plac/parking)
        var stareMiasto = Zone.CreateSingle("Strefa D (Stare Miasto)", scheduleAllWeek, Guid.Parse("0C433454-EAF1-4101-9E5F-CE7C3D3BD894"), Guid.Parse("4CD81C13-2693-4736-8D9D-817A24625506"));

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


