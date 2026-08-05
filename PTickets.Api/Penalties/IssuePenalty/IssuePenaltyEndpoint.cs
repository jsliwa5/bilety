using PTickets.Api.Common;
using PTickets.Api.Inspections.Contracts;
using PTickets.Api.Penalties.Data;
using PTickets.Api.Zones.Contract;

namespace PTickets.Api.Penalties.IssuePenalty;

public static class IssuePenaltyEndpoint
{

    public static void MapIssuePenalty(this WebApplication app)
    {
        app.MapPost("/penalties", IssuePenalty)
        .WithName("IssuePenalty")
        .Produces(StatusCodes.Status200OK);
    }
    private static async Task<IssuePenaltyResponse> IssuePenalty(
        IssuePenaltyRequest request,
        IPenaltyRepository penaltyRepository,
        IInspectionFacade inspectionFacade,
        IZoneFacade zoneFacade
        )
    {
        
        if (!Guid.TryParse(request.InspectionId, out var inspectionIdValue))
        {
            throw new ArgumentException("Invalid inspection ID");
        }

        var inspectionId = new InspectionId(inspectionIdValue);

        var inspection = await inspectionFacade.GetInspectionForPenaltyAsync(inspectionId);

        var calculatedAmount = await zoneFacade.CalculatePenaltyAmountAsync(inspection.StreetId, inspection.InspectionDate);

        var dueDate = inspection.InspectionDate.AddDays(30);

        var penalty = Penalty.Issue(
            inspectionId,
            inspection.RegistrationNumber,
            calculatedAmount,
            dueDate
            );
        await penaltyRepository.AddAsync(penalty);

        return new IssuePenaltyResponse(penalty.Id, calculatedAmount);
    }
}
