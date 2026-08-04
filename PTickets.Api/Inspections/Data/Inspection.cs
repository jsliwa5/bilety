using PTickets.Api.Common;

namespace PTickets.Api.Inspections.Data;

public class Inspection
{
    public InspectionId Id { get; private set; }
    public InspectorId ConductedBy { get; private set; }
    public ZoneId? ZoneId { get; private set; }
    public StreetId StreetId { get; private set; } 
    public RegistrationNumber RegistrationNumber { get; private set; }
    public DateTime InspectionDate { get; private set; }
    public TicketCheckResult Result { get; private set; }
    public InspectionDecision Decision { get; private set; }


    private Inspection() { }

    public static Inspection Conduct(
        InspectorId inspectorId,
        ZoneId? zoneId,
        StreetId streetId,
        RegistrationNumber registrationNumber,
        TicketCheckResult result,
        DateTime inspectionDate)
    {

        var decision = result.IsValid ? InspectionDecision.Approved : InspectionDecision.PenaltyIssued;
        return new Inspection
        {
            Id = new InspectionId(Guid.NewGuid()),
            ConductedBy = inspectorId,
            ZoneId = zoneId,
            StreetId = streetId,
            RegistrationNumber = registrationNumber,
            Result = result,
            Decision = decision,
            InspectionDate = inspectionDate
        };
    }
}
