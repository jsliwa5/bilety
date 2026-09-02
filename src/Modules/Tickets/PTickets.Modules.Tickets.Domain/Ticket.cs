namespace PTickets.Modules.Tickets.Domain;

using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public class Ticket
{
    public Guid Id { get; private set; }
    public RegistrationNumber RegistrationNumber { get; private set; } = null!;
    public StreetId StreetId { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public string ProviderName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private Ticket() { }

    private Ticket(Guid id, RegistrationNumber registrationNumber, StreetId streetId, DateTime validFrom, DateTime validTo, string providerName, DateTime createdAt)
    {
        Id = id;
        RegistrationNumber = registrationNumber;
        StreetId = streetId;
        ValidFrom = validFrom;
        ValidTo = validTo;
        ProviderName = providerName;
        CreatedAt = createdAt;
    }

    public static Ticket Create(RegistrationNumber reg, StreetId streetId, DateTime validFrom, DateTime validTo, string providerName)
    {
        return new Ticket(Guid.NewGuid(), reg, streetId, validFrom, validTo, providerName, DateTime.UtcNow);
    }

    public bool IsValidAt(DateTime dateTime) => dateTime >= ValidFrom && dateTime <= ValidTo;
}

