namespace PTickets.Shared.Contracts.Zones;

using MediatR;

public record StreetCreatedEvent(StreetId StreetId, ZoneId ZoneId, string Name) : INotification;
