namespace PTickets.Modules.Tickets.Infrastructure.Messaging;

using System;
using System.Text.Json.Serialization;

public record RabbitMqTicketMessage(
    [property: JsonPropertyName("ticket_id")] string TicketId,
    [property: JsonPropertyName("license_plate")] string LicensePlate,
    [property: JsonPropertyName("parking_zone")] string? ParkingZone,
    [property: JsonPropertyName("price_pln")] decimal PricePln,
    [property: JsonPropertyName("valid_from")] DateTime ValidFrom,
    [property: JsonPropertyName("valid_to")] DateTime ValidTo,
    [property: JsonPropertyName("issued_at")] DateTime IssuedAt,
    [property: JsonPropertyName("provider")] string Provider
);

