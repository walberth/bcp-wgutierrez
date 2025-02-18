#!/bin/bash

echo "Waiting for Kafka to be ready..."
until kafka-topics --list --bootstrap-server kafka:29092 > /dev/null 2>&1; do
    echo "Kafka is not ready. Retrying in 5 seconds..."
    sleep 5
done

echo "Kafka is ready. Creating topic if not exists..."

# Create the topic
kafka-topics --create \
  --topic consultation-topic \
  --partitions 1 \
  --replication-factor 1 \
  --if-not-exists \
  --bootstrap-server kafka:29092

echo "Topic 'consultation-topic' created (if it did not exist)."
