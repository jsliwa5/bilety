using MediatR;

namespace PTickets.Modules.Inspections.Application.Commands.IssueNotice;

public record IssueNoticeCommand(Guid InspectionId) : IRequest<Guid>;

