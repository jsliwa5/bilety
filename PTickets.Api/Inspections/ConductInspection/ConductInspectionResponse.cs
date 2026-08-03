using PTickets.Api.Common;
using PTickets.Api.Inspections.Data;

namespace PTickets.Api.Inspections.ConductInspection;

public record ConductInspectionResponse
    (
        InspectionId id,
        InspectionDecision decision,
        TicketCheckResult ticketCheckResult
    );
