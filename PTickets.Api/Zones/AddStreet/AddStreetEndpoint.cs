using PTickets.Api.Common;
using PTickets.Api.Zones.Data;

namespace PTickets.Api.Zones.AddStreet;

public static class AddStreetEndpoint
{
    public static void MapAddStreet(this WebApplication app)
    {
        app.MapPost("/streets", AddStreet)
            .WithName("AddStreet")
            .Produces<AddStreetResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> AddStreet(
        AddStreetRequest request,
        IZoneRepository zoneRepository,
        IStreetRepository streetRepository)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Nazwa strefy nie może być pusta.");

        // Jeżeli którykolwiek ze szczegółów harmonogramu jest podany,
        // wymagamy, żeby wszystkie trzy (StartTime, EndTime, PaidDays) były poprawne.
        PaidParkingSchedule? schedule = null;
        var anyScheduleFieldProvided = !string.IsNullOrWhiteSpace(request.StartTime)
                                       || !string.IsNullOrWhiteSpace(request.EndTime)
                                       || !string.IsNullOrWhiteSpace(request.PaidDays);

        if (anyScheduleFieldProvided)
        {
            if (string.IsNullOrWhiteSpace(request.StartTime) || string.IsNullOrWhiteSpace(request.EndTime) || string.IsNullOrWhiteSpace(request.PaidDays))
                return Results.BadRequest("Jeżeli podajesz harmonogram płatnego parkowania, musisz podać StartTime, EndTime i PaidDays.");

            // Parsowanie czasu (akceptujemy formaty typu HH:mm lub HH:mm:ss)
            if (!TimeOnly.TryParse(request.StartTime, out var startTime))
                return Results.BadRequest("StartTime musi być prawidłową godziną (np. '08:00').");

            if (!TimeOnly.TryParse(request.EndTime, out var endTime))
                return Results.BadRequest("EndTime musi być prawidłową godziną (np. '18:00').");

            // Parsowanie dni (oczekujemy listy rozdzielonej przecinkami, np. 'Monday,Tuesday' lub '1,2')
            var parts = request.PaidDays.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
                return Results.BadRequest("PaidDays musi zawierać co najmniej jeden dzień tygodnia.");

            var days = new List<DayOfWeek>();
            foreach (var p in parts)
            {
                if (Enum.TryParse<DayOfWeek>(p, true, out var dow))
                {
                    days.Add(dow);
                    continue;
                }

                // Spróbuj sparsować jako liczba (0 = Sunday, 1 = Monday, ...)
                if (int.TryParse(p, out var num) && num >= 0 && num <= 6)
                {
                    days.Add((DayOfWeek)num);
                    continue;
                }

                return Results.BadRequest($"Nieprawidłowy dzień w PaidDays: '{p}'. Użyj nazw dni lub numerów 0-6.");
            }

            try
            {
                schedule = new PaidParkingSchedule(startTime, endTime, days.ToArray());
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }

        if (!Guid.TryParse(request.ZoneId, out var zoneIdValue))
            throw new ArgumentException("ZoneId musi być poprawnym GUID.");

        var zoneId = new ZoneId(zoneIdValue);
        if(!await zoneRepository.ExistsAsync(zoneId))
            return Results.BadRequest("Strefa o podanym ID nie istnieje.");

        var street = Street.Create(
            zoneId,
            request.Name,
            false,
            schedule
            );

        await streetRepository.AddAsync(street);

        var response = new AddStreetResponse(street.Id, street.Name);
        return Results.Created($"/streets/{street.Id.Value}", response);
    }
}
