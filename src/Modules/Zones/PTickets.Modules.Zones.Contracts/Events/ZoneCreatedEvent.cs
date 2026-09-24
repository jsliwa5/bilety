namespace PTickets.Modules.Zones.Contracts.Events;

using MediatR;
using PTickets.Shared;

public record ZoneCreatedEvent(ZoneId ZoneId, string Name) : INotification;
