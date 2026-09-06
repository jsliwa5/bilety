using MediatR;
using PTickets.Modules.Inspections.Application.Commands.CheckTicket;

namespace PTickets.Modules.Inspections.Application.Commands.ConductSecondCheck;

public record ConductSecondCheckCommand(Guid InspectionId) : IRequest<TicketCheckResultDto>;

