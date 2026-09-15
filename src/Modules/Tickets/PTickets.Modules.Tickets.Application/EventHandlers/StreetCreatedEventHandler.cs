namespace PTickets.Modules.Tickets.Application.EventHandlers;

using MediatR;
using PTickets.Modules.Tickets.Domain;
using PTickets.Shared.Contracts.Zones;

public class StreetCreatedEventHandler(IStreetZoneMappingRepository mappingRepository) : INotificationHandler<StreetCreatedEvent>
{
    public async Task Handle(StreetCreatedEvent notification, CancellationToken cancellationToken)
    {
        var existing = await mappingRepository.GetByStreetIdAsync(notification.StreetId, cancellationToken);
        if (existing is null)
        {
            var mapping = StreetZoneMapping.Create(notification.StreetId, notification.ZoneId);
            await mappingRepository.AddAsync(mapping, cancellationToken);
        }
    }
}

