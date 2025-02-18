#!/bin/bash

# Wait for Kafka broker to be ready (optional, but recommended)
echo "Waiting for Kafka to be ready..."
sleep 10

# Create the topic
kafka-topics --create \
  --topic consultation-topic \
  --partitions 1 \
  --replication-factor 1 \
  --if-not-exists \
  --bootstrap-server kafka:29092

echo "Topic 'consultation-topic' created (if it did not exist)."
