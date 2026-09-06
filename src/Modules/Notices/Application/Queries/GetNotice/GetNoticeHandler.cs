using MediatR;
using PTickets.Modules.Notices.Domain;
using PTickets.Shared;

namespace PTickets.Modules.Notices.Application.Queries.GetNotice;

public class GetNoticeHandler : IRequestHandler<GetNoticeQuery, NoticeDto?>
{
    private readonly INoticeRepository _repository;

    public GetNoticeHandler(INoticeRepository repository)
    {
        _repository = repository;
    }

    public async Task<NoticeDto?> Handle(GetNoticeQuery request, CancellationToken cancellationToken)
    {
        var notice = await _repository.GetByIdAsync(new NoticeId(request.NoticeId), cancellationToken);

        if (notice == null)
            return null;

        return new NoticeDto(
            notice.Id.Value,
            notice.InspectionId.Value,
            notice.RegistrationNumber.Value,
            notice.TotalAmount,
            notice.IssuedAt
        );
    }
}
