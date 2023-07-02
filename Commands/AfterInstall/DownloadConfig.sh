#!/usr/bin/env bash
SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

if ! INSTAR_CONF_FILE=$("$SCRIPT_DIR"/../Global/GetInstarConfiguration.sh); then
  echo "Could not get config file."
  exit 1
fi 

aws s3api get-object --bucket instarconfig --region us-west-2 --key "$INSTAR_CONF_FILE" "/Instar/bin/Config/$INSTAR_CONF_FILE"