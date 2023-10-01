#!/usr/bin/env bash

TAG_NAME="Instar"
EC2_INSTANCE="$(ec2-metadata -i | awk '{print $2}')"
EC2_REGION="$(ec2-metadata -z | awk '{print $2}' | sed -E 's/([A-Z]+-[A-Z]+-[0-9]+).*/\1/gI')"
INSTAR_STAGE="$(aws ec2 describe-tags --filters "Name=resource-id,Values=$EC2_INSTANCE" "Name=key,Values=$TAG_NAME" --region $EC2_REGION --output=text | awk '{print $5}')"

if [ "$INSTAR_STAGE" = "Production" ]; then
  echo "Instar.conf.json"
  exit 0
elif [ "$INSTAR_STAGE" = "Test" ]; then
  echo "Instar.debug.conf.json"
  exit 0
else
  exit 1
fi
