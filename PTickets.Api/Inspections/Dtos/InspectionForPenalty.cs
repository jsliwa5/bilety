using PTickets.Api.Common;

namespace PTickets.Api.Inspections.Dtos;

public record InspectionForPenalty(
    InspectionId InspectionId,
    StreetId StreetId,
    RegistrationNumber RegistrationNumber,
    DateTime InspectionDate);
