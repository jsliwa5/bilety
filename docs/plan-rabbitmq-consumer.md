# Plan: RabbitMQ Ticket Consumer w module Tickets

## Założenia

- **Bez stref/ulic w biletach** — sprawdzamy tylko, czy dana rejestracja ma ważny bilet (bez powiązania z `StreetId`)
- Consumer wewnątrz modułu `Tickets` (warstwa Infrastructure)
- Classic queues (bez quorum)
- At-least-once delivery → idempotentność po stronie consumenta

---

## Krok 1: Poprawki konfiguracji RabbitMQ

### 1a. TTL na DLQ (7 dni)

Plik: `definitions.template.json` — zmienić definicję kolejki `parking.tickets.dlq`:
```json
{
  "name": "parking.tickets.dlq",
  "vhost": "parking",
  "durable": true,
  "auto_delete": false,
  "arguments": {
    "x-message-ttl": 604800000
  }
}
```

### 1b. Retry z opóźnieniem (parking lot pattern)

Dodać nową kolejkę `parking.tickets.retry` z TTL i rebindem do głównej kolejki:

```json
{
  "name": "parking.tickets.retry",
  "vhost": "parking",
  "durable": true,
  "auto_delete": false,
  "arguments": {
    "x-message-ttl": 30000,
    "x-dead-letter-exchange": "parking.tickets.topic",
    "x-dead-letter-routing-key": "ticket.purchase.retry"
  }
}
```

Dodać binding retry → główna kolejka:
```json
{
  "source": "parking.tickets.topic",
  "vhost": "parking",
  "destination": "parking.controller-sync",
  "destination_type": "queue",
  "routing_key": "ticket.purchase.retry",
  "arguments": {}
}
```

Dodać nowy exchange + binding do retry queue (consumer NACKuje do tego exchange):
```json
{
  "name": "parking.tickets.retry-exchange",
  "vhost": "parking",
  "type": "fanout",
  "durable": true
}
```
```json
{
  "source": "parking.tickets.retry-exchange",
  "destination": "parking.tickets.retry",
  "destination_type": "queue"
}
```

W consumerze: jeśli `retry_count < 3` → NACK do retry exchange, jeśli `>= 3` → NACK do DLQ.

---

## Krok 2: Uproszczenie encji `Ticket`

Plik: `src/Modules/Tickets/PTickets.Modules.Tickets.Domain/Ticket.cs`

Zmiany:
- Dodać pole `string? ExternalTicketId` (do idempotentności — `ticket_id` z RabbitMQ)
- `StreetId` zmienić na nullable (`StreetId?`)
- Dodać `string? ParkingZone` (surowa wartość z wiadomości, do późniejszego mapowania)
- Dodać nową metodę fabryczną `CreateFromExternal(...)` bez wymagania `StreetId`

```csharp
public static Ticket CreateFromExternal(
    string externalTicketId,
    RegistrationNumber reg,
    DateTime validFrom,
    DateTime validTo,
    string providerName,
    string? parkingZone = null)
{
    return new Ticket(
        Guid.NewGuid(), reg, streetId: null,
        validFrom, validTo, providerName, DateTime.UtcNow)
    {
        ExternalTicketId = externalTicketId,
        ParkingZone = parkingZone
    };
}
```

---

## Krok 3: Rozszerzenie `ITicketRepository`

Plik: `src/Modules/Tickets/PTickets.Modules.Tickets.Domain/ITicketRepository.cs`

Dodać:
```csharp
Task<bool> ExistsByExternalIdAsync(string externalTicketId, CancellationToken ct = default);
Task<Ticket?> FindActiveTicketByRegistrationAsync(RegistrationNumber reg, DateTime at, CancellationToken ct = default);
```

Implementacja w `EfTicketRepository.cs`:
- `ExistsByExternalIdAsync` → `AnyAsync(t => t.ExternalTicketId == externalTicketId)`
- `FindActiveTicketByRegistrationAsync` → jak `FindActiveTicketAsync` ale **bez filtrowania po `StreetId`**

---

## Krok 4: DTO wiadomości RabbitMQ

Nowy plik: `src/Modules/Tickets/PTickets.Modules.Tickets.Infrastructure/Messaging/RabbitMqTicketMessage.cs`

```csharp
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
```

---

## Krok 5: BackgroundService — consumer

Nowy plik: `src/Modules/Tickets/PTickets.Modules.Tickets.Infrastructure/Messaging/RabbitMqTicketConsumer.cs`

Szkielet:
```
class RabbitMqTicketConsumer : BackgroundService

ExecuteAsync:
  1. Odczytać konfigurację RabbitMQ z IConfiguration
  2. Utworzyć ConnectionFactory → IConnection → IChannel
  3. channel.BasicQos(prefetchCount: 10)
  4. Zarejestrować AsyncEventingBasicConsumer na "parking.controller-sync"
  5. W KAŻDYM RECEIVED:
     a) Deserializować body → RabbitMqTicketMessage
     b) Otworzyć IServiceScope → pobrać ITicketRepository
     c) Sprawdzić duplikat: ExistsByExternalIdAsync(msg.TicketId)
     d) Jeśli nowy → Ticket.CreateFromExternal(...) + AddAsync + SaveChanges
     e) ACK wiadomość
     f) W razie wyjątku:
        - Odczytać retry_count z headers
        - Jeśli < 3 → republish do retry exchange z retry_count+1
        - Jeśli >= 3 → NACK bez requeue (trafi do DLQ)
     g) Logować każdą operację

StopAsync:
  Zamknąć channel + connection
```

> **Ważne:** BackgroundService żyje jako singleton, ale `DbContext` jest scoped.
> Dlatego w każdym `Received` trzeba tworzyć nowy `IServiceScope`.

---

## Krok 6: Konfiguracja

Plik: `src/PTickets.Api/appsettings.json` — dodać sekcję:
```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "VirtualHost": "parking",
    "UserName": "controller_app",
    "Password": "ControllerPass123!",
    "QueueName": "parking.controller-sync"
  }
}
```

Opcjonalnie: klasa `RabbitMqOptions` + `services.Configure<RabbitMqOptions>(...)`.

---

## Krok 7: Rejestracja w DI

Plik: `src/Modules/Tickets/PTickets.Modules.Tickets.Infrastructure/TicketsModule.cs`

Dodać:
```csharp
services.AddHostedService<RabbitMqTicketConsumer>();
```

---

## Krok 8: Aktualizacja `TicketVerificationService`

Plik: `src/Modules/Tickets/PTickets.Modules.Tickets.Application/Services/TicketVerificationService.cs`

Dodać **nowy krok** przed odpytywaniem providerów:
```
1. Karta Mieszkańca       → FindActiveCardAsync(reg, streetId, at)
2. Lokalny bilet (street) → FindActiveTicketAsync(reg, streetId, at)       ← istniejący
3. Lokalny bilet (any)    → FindActiveTicketByRegistrationAsync(reg, at)   ← NOWY
4. Provider zewnętrzny    → ITicketProvider.CheckAsync(...)                ← fallback
```

Dzięki temu bilety z RabbitMQ (bez `StreetId`) też zostaną znalezione.

---

## Krok 9: Migracja bazy danych

Nowe kolumny w tabeli `Tickets`:
- `ExternalTicketId` (string?, nullable, unique index)
- `ParkingZone` (string?, nullable)
- `StreetId` zmienić na nullable

Komenda:
```bash
dotnet ef migrations add AddExternalTicketFields \
  --project src/Modules/Tickets/PTickets.Modules.Tickets.Infrastructure \
  --startup-project src/PTickets.Api
```

---

## Krok 10: Pakiet NuGet

Plik: `PTickets.Modules.Tickets.Infrastructure.csproj`

```xml
<PackageReference Include="RabbitMQ.Client" Version="7.*" />
```

---

## Kolejność realizacji

1. **Krok 10** — dodać pakiet NuGet
2. **Krok 2** — rozszerzyć encję `Ticket`
3. **Krok 3** — rozszerzyć repozytorium
4. **Krok 9** — migracja bazy
5. **Krok 6** — konfiguracja appsettings
6. **Krok 4** — DTO wiadomości
7. **Krok 5** — BackgroundService consumer
8. **Krok 7** — rejestracja w DI
9. **Krok 8** — aktualizacja TicketVerificationService
10. **Krok 1** — poprawki RabbitMQ (definitions.json)

---

## Poza zakresem (na później)

- Mapowanie `parking_zone` → `StreetId`/`ZoneId`
- Health check dla połączenia z RabbitMQ
- Metryki (consumed/s, duplikaty, błędy)
- Reconnect policy (RabbitMQ.Client 7.x ma wbudowany auto-recovery)

