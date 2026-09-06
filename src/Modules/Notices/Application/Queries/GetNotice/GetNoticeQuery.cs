using MediatR;

namespace PTickets.Modules.Notices.Application.Queries.GetNotice;

public record GetNoticeQuery(Guid NoticeId) : IRequest<NoticeDto?>;

