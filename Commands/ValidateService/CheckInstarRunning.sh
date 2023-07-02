#!/usr/bin/env bash

# Wait for 30 seconds before checking.
# Sometimes Instar is running while it tries to
# compile error reports.
sleep 30

ps -C InstarBot
if [ $? -ne 0 ]; then
  echo "InstarBot did not start successfully."
  exit 1
fi

exit 0