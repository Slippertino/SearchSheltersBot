#!/bin/sh
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P SaPassword_1 -d SsbotDb -W -u -h-1 -s@ -Q "set nocount on; select * from SsbotDb.dbo.Shelters" -o "/data/shelters.csv"
echo "Shelters backup updated" >> /var/log/cron.log 2>&1
