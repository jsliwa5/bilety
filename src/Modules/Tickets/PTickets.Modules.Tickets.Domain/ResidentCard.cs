namespace PTickets.Modules.Tickets.Domain;

using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public class ResidentCard
{
    public Guid Id { get; private set; }
    public RegistrationNumber RegistrationNumber { get; private set; } = null!;
    public StreetId StreetId { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ResidentCard() { }

    private ResidentCard(Guid id, RegistrationNumber registrationNumber, StreetId streetId, DateTime validFrom, DateTime validTo, DateTime createdAt)
    {
        Id = id;
        RegistrationNumber = registrationNumber;
        StreetId = streetId;
        ValidFrom = validFrom;
        ValidTo = validTo;
        CreatedAt = createdAt;
    }

    public static ResidentCard Create(RegistrationNumber registrationNumber, StreetId streetId, DateTime validFrom, DateTime validTo)
    {
        if (validFrom >= validTo)
        {
            throw new ArgumentException("ValidTo must be greater than ValidFrom.");
        }
        
        return new ResidentCard(Guid.NewGuid(), registrationNumber, streetId, validFrom, validTo, DateTime.UtcNow);
    }

    public bool IsValidAt(DateTime dateTime) => dateTime >= ValidFrom && dateTime <= ValidTo;
}

