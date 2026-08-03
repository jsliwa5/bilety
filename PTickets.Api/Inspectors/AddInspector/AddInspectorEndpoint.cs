using PTickets.Api.Common;
using PTickets.Api.Inspectors.Data;
using PTickets.Api.Zones.Contract;

namespace PTickets.Api.Inspectors.AddInspector;

public static class AddInspectorEndpoint
{
    public static void MapAddInspector(this WebApplication app)
    {
        app.MapPost("/inspectors", AddInspector)
            .WithName("AddInspector")
            .Produces<AddInspectorResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> AddInspector(
        AddInspectorRequest request,
        IInspectorRepository inspectorRepository,
        IZoneFacade zoneFacade)
    {
        // Walidacja imienia
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Imię inspektora nie może być puste.");

        // Parsowanie AssignedToZoneId (opcjonalnie)
        ZoneId? assignedToZoneId = null;
        if (!string.IsNullOrWhiteSpace(request.AssignedToZoneId))
        {
            if (!Guid.TryParse(request.AssignedToZoneId, out var zoneIdValue))
                return Results.BadRequest("AssignedToZoneId musi być poprawnym GUID.");
            assignedToZoneId = new ZoneId(zoneIdValue);
        }

        // Jeśli podano AssignedToZoneId, zweryfikuj czy strefa istnieje
        if (assignedToZoneId != null)
        {
            var exists = await zoneFacade.ExistsByIdAsync(assignedToZoneId.Value);
            if (!exists)
                return Results.BadRequest("AssignedToZoneId wskazuje na nieistniejącą strefę.");
        }

        // Tworzenie inspektora
        var inspector = Inspector.Create(request.Name, assignedToZoneId);

        // Zapisanie do bazy
        await inspectorRepository.AddAsync(inspector);

        var response = new AddInspectorResponse(inspector.Id);
        return Results.Created($"/inspectors/{inspector.Id.Value}", response);
    }
}
