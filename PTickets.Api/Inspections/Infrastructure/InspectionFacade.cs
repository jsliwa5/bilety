using PTickets.Api.Common;
using PTickets.Api.Inspections.Contracts;
using PTickets.Api.Inspections.Data;
using PTickets.Api.Inspections.Dtos;

namespace PTickets.Api.Inspections.Infrastructure;

public class InspectionFacade : IInspectionFacade
{

    private readonly IInspectionRepository _inspectionRepository;

    public InspectionFacade(IInspectionRepository inspectionRepository)
    {
        _inspectionRepository = inspectionRepository;
    }
    public async Task<InspectionForPenalty> GetInspectionForPenaltyAsync(InspectionId inspectionId)
    {
        var inspection = await _inspectionRepository.GetByIdAsync(inspectionId);

        if (inspection == null) throw new ArgumentNullException(nameof(inspection));

        return new InspectionForPenalty(
            inspection.Id,
            inspection.StreetId,
            inspection.RegistrationNumber,
            inspection.InspectionDate
        );
    }
}
