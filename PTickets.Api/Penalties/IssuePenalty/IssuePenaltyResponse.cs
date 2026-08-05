using PTickets.Api.Common;

namespace PTickets.Api.Penalties.IssuePenalty;

public record IssuePenaltyResponse(PenaltyId Id, decimal Amount);

