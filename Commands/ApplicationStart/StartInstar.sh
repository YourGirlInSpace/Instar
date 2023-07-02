#!/usr/bin/env bash


startInstar() {
  local INSTAR_CONF_FILE=$(../Global/GetInstarConfiguration.sh)
  
  if [ $? -ne 0 ]; then
    echo "Could not get config file."
    exit 1
  fi 
  
  local INSTAR_CONF="/Instar/bin/Config/$INSTAR_CONF_FILE"

  /Instar/bin/InstarBot --config-path $INSTAR_CONF > /dev/null 2> /dev/null < /dev/null &
}

startInstar