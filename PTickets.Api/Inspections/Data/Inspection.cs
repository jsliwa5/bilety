using PTickets.Api.Common;

namespace PTickets.Api.Inspections.Data;

public class Inspection
{
    public InspectionId Id { get; private set; }
    public InspectorId ConductedBy { get; private set; }
    public ZoneId ZoneId { get; private set; }
    public StreetId? StreetId { get; private set; } // Opcjonalne!
    public RegistrationNumber RegistrationNumber { get; private set; }
    public DateTime InspectionDate { get; private set; }
    public TicketCheckResult Result { get; private set; }
    public InspectionDecision Decision { get; private set; }

    // Prywatny konstruktor wymusza tworzenie przez metodę fabrykującą (Factory Method)
    private Inspection() { }

    public static Inspection Create(
        InspectorId inspectorId,
        ZoneId zoneId,
        StreetId? streetId,
        RegistrationNumber registrationNumber,
        TicketCheckResult result,
        InspectionDecision decision,
        DateTime inspectionDate)
    {
        // Tutaj walidujemy reguły tworzenia kontroli
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
