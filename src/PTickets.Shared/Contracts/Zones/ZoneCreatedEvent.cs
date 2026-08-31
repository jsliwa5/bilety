namespace PTickets.Shared.Contracts.Zones;

using MediatR;

public record ZoneCreatedEvent(ZoneId ZoneId, string Name) : INotification;
