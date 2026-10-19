namespace PTickets.Modules.Inspections.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InspectorNotAssignedToZoneException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;

    public InspectorNotAssignedToZoneException()
        : base("Kontroler jest przypisany do innej strefy i nie może przeprowadzić kontroli w wybranej strefie.")
    {
    }
}

