namespace PTickets.Modules.Zones.Contracts.Events;

using MediatR;
using PTickets.Shared;

public record StreetCreatedEvent(StreetId StreetId, ZoneId ZoneId, string Name) : INotification;
