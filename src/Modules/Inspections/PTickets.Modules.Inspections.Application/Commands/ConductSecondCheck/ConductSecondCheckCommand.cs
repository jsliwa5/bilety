using MediatR;
using PTickets.Modules.Inspections.Application.Commands.CheckTicket;

namespace PTickets.Modules.Inspections.Application.Commands.ConductSecondCheck;

[Obsolete("Use StartInspection + CheckTicket flow instead. The second check is now detected automatically.")]
public record ConductSecondCheckCommand(Guid InspectionId) : IRequest<TicketCheckResultDto>;

