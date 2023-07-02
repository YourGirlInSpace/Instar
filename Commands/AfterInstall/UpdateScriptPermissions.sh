#!/usr/bin/env bash

echo "Updating permissions for GetInstarConfiguration.sh..."

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
chmod +x "$SCRIPT_DIR/../Global/GetInstarConfiguration.sh"