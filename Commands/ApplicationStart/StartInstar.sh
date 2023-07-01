#!/usr/bin/env bash

startInstar() {
  local TAG_NAME="Instar"
  local EC2_INSTANCE="`ec2-metadata -i | awk '{print $2}'`"
  local EC2_REGION="`ec2-metadata -z | awk '{print $2}' | sed -E 's/([A-Z]+-[A-Z]+-[0-9]+).*/\1/gI'`"
  local INSTAR_STAGE="`aws ec2 describe-tags --filters "Name=resource-id,Values=$EC2_INSTANCE" "Name=key,Values=$TAG_NAME" --region $EC2_REGION --output=text | awk '{print $5}'`"
  
  local INSTAR_CONF_DIR="/Instar/bin/Config"
  if [ $INSTAR_STAGE = "Production" ]; then
    INSTAR_CONF="$INSTAR_CONF_DIR/Instar.conf.json"
  elif [ $INSTAR_STAGE = "Test" ]; then
    INSTAR_CONF="$INSTAR_CONF_DIR/Instar.debug.conf.json"
  else
    echo "Failed to determine correct Instar configuration"
    exit 1
  fi
  
  /Instar/bin/InstarBot --config-path $INSTAR_CONF > /dev/null 2> /dev/null < /dev/null &
}

startInstar