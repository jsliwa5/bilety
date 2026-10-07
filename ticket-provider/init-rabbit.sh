#!/bin/bash
set -e

perl -pe 's/\$\{([A-Z_]+)\}/$ENV{$1}/ge' < /etc/rabbitmq/definitions.template.json > /etc/rabbitmq/definitions.json

exec docker-entrypoint.sh rabbitmq-server