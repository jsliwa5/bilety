using PTickets.Api.Common;

namespace PTickets.Api.Inspections.ConductInspection;

public record ConductInspectionRequest(
    string InspectorId,
    string ZoneId,
    string? StreetId,
    string RegistrationNumber
);
