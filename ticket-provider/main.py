import os
import json
import time
import uuid
import random
from datetime import datetime, timezone, timedelta
import pika
from pika.exceptions import AMQPError, NackError, UnroutableError


RABBIT_HOST = "127.0.0.1"
RABBIT_PORT = 5672
RABBIT_VHOST = "parking"
RABBIT_USER = "provider_simulator"
RABBIT_PASS = "PROVIDER123"
print(RABBIT_PASS)
EXCHANGE_NAME = "parking.tickets.topic"


ZONES = [
    # Strefy (bilet ważny na wszystkich ulicach w danej strefie)
    "A2F7ACB7-C376-4CBF-9AE9-D74DE9D1E08A", # Strefa A (Centrum)
    "BA4EC572-0D74-47ED-83B9-C63113449F61", # Strefa B (Mokotów)
    "3626A1D9-1C1B-499B-97B2-ACC5F9989923", # Strefa C (Wola)
    "0C433454-EAF1-4101-9E5F-CE7C3D3BD894", # Strefa D (Stare Miasto)
    # Konkretne ulice (bilet ważny tylko na tej ulicy)
    "4D2EF8BF-88B8-4554-87BA-6D29B2AE4814", # Marszałkowska (Strefa A)
    "895838BE-9EA2-465E-8BEA-3351D2318B41"  # Puławska (Strefa B)
]
CITIES = ["RZ", "WA", "GD", "LU"]


def generate_ticket() -> tuple[dict, str]:

    ticket_id = str(uuid.uuid4())
    zone = random.choice(ZONES)
    plate = f"{random.choice(CITIES)} {random.randint(10000, 99999)}"

    now = datetime.now(timezone.utc)
    duration_hours = random.choice([1, 2, 4, 8])
    valid_to = now + timedelta(hours=duration_hours)

    payload = {
        "ticket_id": ticket_id,
        "license_plate": plate,
        "parking_zone": zone,
        "price_pln": round(duration_hours * random.uniform(4.5, 7.0), 2),
        "valid_from": now.isoformat(),
        "valid_to": valid_to.isoformat(),
        "issued_at": now.isoformat(),
        "provider": "Szwagrex usługi budowlano-biletowe"
    }

    routing_key = f"ticket.purchase.{zone}"
    return payload, routing_key


def main():
    credentials = pika.PlainCredentials(RABBIT_USER, RABBIT_PASS)
    parameters = pika.ConnectionParameters(
        host=RABBIT_HOST,
        port=RABBIT_PORT,
        virtual_host=RABBIT_VHOST,
        credentials=credentials,
        heartbeat=60,
        blocked_connection_timeout=300
    )

    print(f"[*] Łączenie z RabbitMQ na vhostzie '/{RABBIT_VHOST}' jako '{RABBIT_USER}'...")
    connection = pika.BlockingConnection(parameters)
    channel = connection.channel()

    channel.confirm_delivery()
    print("[*] Publisher Confirms włączone. Rozpoczynam nadawanie biletów (Ctrl+C aby przerwać)...")

    try:
        while True:
            ticket, routing_key = generate_ticket()
            message_body = json.dumps(ticket)

            # delivery_mode=2 oznacza wiadomość trwałą (persistent)
            properties = pika.BasicProperties(
                content_type="application/json",
                delivery_mode=pika.DeliveryMode.Persistent,
                message_id=ticket["ticket_id"],
                timestamp=int(time.time()),
                headers={"source": "simulator", "retry_count": 0}
            )

            try:
                # W trybie confirm_delivery pika zgłasza NackError/UnroutableError w razie problemu
                channel.basic_publish(
                    exchange=EXCHANGE_NAME,
                    routing_key=routing_key,
                    body=message_body.encode("utf-8"),
                    properties=properties,
                    mandatory=True  # Rzuci błąd, jeśli żaden binding nie obsłuży klucza
                )
                print(
                    f"[✓] ACK: Wysłano bilet {ticket['ticket_id'][:8]}... | Rej: {ticket['license_plate']} | Klucz: {routing_key}")

            except (NackError, UnroutableError) as publish_err:
                print(f"[X] Błąd publikacji wiadomości: {publish_err}")

            # Odstęp między generowaniem kolejnych biletów (10-30 sekundy)
            time.sleep(random.uniform(1.0, 3.0) * 10)

    except KeyboardInterrupt:
        print("\n[*] Zatrzymywanie generatora...")
    finally:
        if connection.is_open:
            connection.close()
            print("[*] Połączenie AMQP zamknięte.")


if __name__ == "__main__":
    main()