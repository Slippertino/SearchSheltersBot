#!/bin/sh
mkdir -p /data
/opt/mssql/bin/sqlservr &
sleep 5
cron -f