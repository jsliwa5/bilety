namespace PTickets.Modules.Tickets.Domain;

using PTickets.Modules.Tickets.Domain.Exceptions;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public class Ticket
{
    public Guid Id { get; private set; }
    public string? ExternalTicketId { get; private set; }
    public RegistrationNumber RegistrationNumber { get; private set; } = null!;
    public StreetId? StreetId { get; private set; }
    public string? ParkingZone { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public string ProviderName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private Ticket() { }

    private Ticket(Guid id, RegistrationNumber registrationNumber, StreetId? streetId, DateTime validFrom, DateTime validTo, string providerName, DateTime createdAt)
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
        if (streetId == PTickets.Shared.StreetId.Empty)
            throw new InvalidTicketDataException("StreetId cannot be empty for a street-bound ticket.");

        if (validFrom >= validTo)
            throw new InvalidTicketDatesException();

        if (string.IsNullOrWhiteSpace(providerName))
            throw new InvalidTicketDataException("ProviderName cannot be empty.");

        return new Ticket(Guid.NewGuid(), reg, streetId, validFrom, validTo, providerName.Trim(), DateTime.UtcNow);
    }

    public static Ticket CreateFromExternal(
        string externalTicketId,
        RegistrationNumber reg,
        DateTime validFrom,
        DateTime validTo,
        string providerName,
        string? parkingZone = null)
    {
        if (string.IsNullOrWhiteSpace(externalTicketId))
            throw new InvalidTicketDataException("ExternalTicketId cannot be empty.");

        if (validFrom >= validTo)
            throw new InvalidTicketDatesException();

        if (string.IsNullOrWhiteSpace(providerName))
            throw new InvalidTicketDataException("ProviderName cannot be empty.");

        return new Ticket(Guid.NewGuid(), reg, null, validFrom, validTo, providerName.Trim(), DateTime.UtcNow)
        {
            ExternalTicketId = externalTicketId.Trim(),
            ParkingZone = string.IsNullOrWhiteSpace(parkingZone) ? null : parkingZone.Trim()
        };
    }

    public bool IsValidAt(DateTime dateTime) => dateTime >= ValidFrom && dateTime <= ValidTo;
}
