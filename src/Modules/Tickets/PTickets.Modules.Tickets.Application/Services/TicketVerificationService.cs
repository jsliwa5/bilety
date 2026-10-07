namespace PTickets.Modules.Tickets.Application.Services;

using MediatR;
using PTickets.Modules.Tickets.Application.Commands;
using PTickets.Modules.Tickets.Domain;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public class TicketVerificationService(
    IStreetZoneMappingRepository mappingRepository,
    IResidentCardRepository residentCardRepository,
    ITicketRepository ticketRepository,
    IEnumerable<ITicketProvider> ticketProviders,
    ISender sender)
{
    public async Task<TicketCheckResult> VerifyTicketAsync(
        RegistrationNumber registration,
        StreetId streetId,
        DateTime at,
        CancellationToken ct = default)
    {
        // 1. Check Resident Card (Fast path)
        var residentCard = await residentCardRepository.FindActiveCardAsync(registration, streetId, at, ct);
        if (residentCard is not null)
        {
            return TicketCheckResult.Valid(residentCard.ValidFrom, residentCard.ValidTo, "Karta Mieszkańca");
        }

        // 2. Check local tickets (specific to street)
        var localTicket = await ticketRepository.FindActiveTicketAsync(registration, streetId, at, ct);
        if (localTicket is not null && localTicket.IsValidAt(at))
        {
            return TicketCheckResult.Valid(localTicket.ValidFrom, localTicket.ValidTo, localTicket.ProviderName);
        }

        // 3. Check local tickets (any street - e.g. from RabbitMQ)
        var mapping = await mappingRepository.GetByStreetIdAsync(streetId, ct);
        var activeTicketsAny = await ticketRepository.FindActiveTicketsByRegistrationAsync(registration, at, ct);
        
        foreach (var ticket in activeTicketsAny)
        {
            if (string.IsNullOrEmpty(ticket.ParkingZone))
                continue;

            var parkingZoneUpper = ticket.ParkingZone.ToUpperInvariant();
            var matchesStreetId = parkingZoneUpper == streetId.Value.ToString().ToUpperInvariant();
            var matchesZoneId = mapping != null && parkingZoneUpper == mapping.ZoneId.Value.ToString().ToUpperInvariant();

            if (matchesStreetId || matchesZoneId)
            {
                if (ticket.IsValidAt(at))
                {
                    return TicketCheckResult.Valid(ticket.ValidFrom, ticket.ValidTo, ticket.ProviderName);
                }
            }
        }

        foreach (var provider in ticketProviders)
        {
            var result = await provider.CheckAsync(registration, streetId, at, ct);
            if (result.IsValid)
            {
                var validFrom = result.ValidFrom ?? at;
                var validTo = result.ValidTo ?? at;

                await sender.Send(new RecordExternalTicketCommand(
                    registration,
                    streetId,
                    validFrom,
                    validTo,
                    provider.ProviderName), ct);

                return result;
            }
        }

        return TicketCheckResult.Invalid("Brak ważnego biletu");
    }
}

