using PTickets.Api.Common;
using PTickets.Api.Inspections.Contracts;
using PTickets.Api.Inspections.Data;
using PTickets.Api.Zones.Contract;

namespace PTickets.Api.Inspections.ConductInspection;

public static class ConductInspectionEndpoint
{
    public static void MapConductInspection(this WebApplication app)
    {
        app.MapPost("/inspections/conduct", ConductInspection)
            .WithName("ConductInspection")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<ConductInspectionResponse> ConductInspection(
        ConductInspectionRequest request,
        IDateTimeProvider dateTimeProvider,
        IZoneFacade zoneFacade,
        ITicketProviderClient ticketProviderClient,
        IInspectionRepository inspectionRepository)
    {

        if (!Guid.TryParse(request.InspectorId, out var inspectorIdValue))
            throw new ArgumentException("InspectorId musi być poprawnym GUID.");

        if (!Guid.TryParse(request.StreetId, out var streetIdValue))
            throw new ArgumentException("StreetId musi być poprawnym GUID.");

        var inspectorId = new InspectorId(inspectorIdValue);
        var streetId = new StreetId(streetIdValue);

        var registrationNumber = new RegistrationNumber(request.RegistrationNumber);

        var isPaymentRequired = await zoneFacade.IsPaidAtDateTimeAsync(streetId, dateTimeProvider.UtcNow);

        var ticketCheckResult = isPaymentRequired
            ? await ticketProviderClient.CheckTicketAsync(registrationNumber, streetId)
            : TicketCheckResult.TicketNotRequired();

        var inspection = Inspection.Conduct(inspectorId, null, streetId, registrationNumber, ticketCheckResult, dateTimeProvider.UtcNow);

        await inspectionRepository.AddAsync(inspection);
        return new ConductInspectionResponse(inspection.Id, inspection.Decision, ticketCheckResult);
    }
}
