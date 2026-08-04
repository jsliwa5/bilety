using PTickets.Api.Common;

namespace PTickets.Api.Inspections.ConductInspection;

public record ConductInspectionRequest(
    string InspectorId,
    string StreetId,
    string RegistrationNumber
);
