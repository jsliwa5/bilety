using MediatR;

namespace PTickets.Modules.Inspections.Application.Commands.CheckTicket;

public record CheckTicketCommand(Guid InspectionId) : IRequest<TicketCheckResultDto>;
public record TicketCheckResultDto(bool IsValid, string? Message);

