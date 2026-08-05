using PTickets.Api.Common;
using PTickets.Api.Inspections.Dtos;

namespace PTickets.Api.Inspections.Contracts;

public interface IInspectionFacade
{
    Task<InspectionForPenalty> GetInspectionForPenaltyAsync(InspectionId inspectionId);
}
