using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;

namespace PTickets.Modules.Inspections.Application.Queries.GetNotice;

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
        if (notice == null) return null;

        var items = notice.Items.Select(i => new NoticeItemDto(
            i.Id,
            i.ViolationTypeId.Value,
            i.Amount,
            i.Surcharge,
            i.Status.ToString())).ToList();

        return new NoticeDto(
            notice.Id.Value,
            notice.InspectionId.Value,
            notice.RegistrationNumber.Value,
            notice.TotalAmount,
            notice.IssuedAt,
            items);
    }
}

