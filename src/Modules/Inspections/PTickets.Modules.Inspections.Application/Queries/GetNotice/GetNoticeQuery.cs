using MediatR;

namespace PTickets.Modules.Inspections.Application.Queries.GetNotice;

public record GetNoticeQuery(Guid NoticeId) : IRequest<NoticeDto?>;

