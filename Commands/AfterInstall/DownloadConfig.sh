#!/usr/bin/env bash

INSTAR_CONF_FILE=$(../Global/GetInstarConfiguration.sh)

if [ $? -ne 0 ]; then
  echo "Could not get config file."
  exit 1
fi 

aws s3api get-object --bucket instarconfig --region us-west-2 --key $INSTAR_CONF_FILE /Instar/bin/Config/$INSTAR_CONF_FILE