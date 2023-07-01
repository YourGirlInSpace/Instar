#!/usr/bin/env bash

INSTAR_CONF_FILE=Instar.conf.json
aws s3api get-object --bucket instarconfig --region us-west-2 --key $INSTAR_CONF_FILE /Instar/bin/Config/$INSTAR_CONF_FILE