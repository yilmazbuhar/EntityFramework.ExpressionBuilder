#!/bin/bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=bwCPCTjpv2Y0Ww9v" -e "MSSQL_PID=Express" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest
sqlcmd -S "localhost" -U "sa" -P "bwCPCTjpv2Y0Ww9v" -i instnwnd.sql