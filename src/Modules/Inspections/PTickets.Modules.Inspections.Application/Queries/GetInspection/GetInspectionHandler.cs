using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;

namespace PTickets.Modules.Inspections.Application.Queries.GetInspection;

public class GetInspectionHandler : IRequestHandler<GetInspectionQuery, InspectionDto?>
{
    private readonly IInspectionRepository _repository;

    public GetInspectionHandler(IInspectionRepository repository)
    {
        _repository = repository;
    }

    public async Task<InspectionDto?> Handle(GetInspectionQuery request, CancellationToken cancellationToken)
    {
        var inspection = await _repository.GetByIdAsync(new InspectionId(request.InspectionId), cancellationToken);
        if (inspection == null) return null;

        var violations = inspection.Violations.Select(v => new ViolationEntryDto(
            v.Id, 
            v.ViolationTypeId.Value, 
            v.Source.ToString(), 
            v.AddedAt)).ToList();

        return new InspectionDto(
            inspection.Id.Value,
            inspection.SessionId.Value,
            inspection.InspectorId.Value,
            inspection.RegistrationNumber.Value,
            inspection.Status.ToString(),
            violations,
            inspection.NoticeId?.Value);
    }
}

