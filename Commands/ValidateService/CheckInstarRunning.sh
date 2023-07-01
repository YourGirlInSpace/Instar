#!/usr/bin/env bash

ps -C InstarBot
if [ $? -ne 0 ]; then
  echo "InstarBot did not start successfully."
  exit 1
fi

exit 0