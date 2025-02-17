#!/bin/bash

/wait-for-it.sh sql_server:1433 --timeout=240 --strict -- sleep 30

/opt/mssql-tools/bin/sqlcmd -S sql_server,1433 -U SA -P "sql2016." -i /usr/local/bin/sql_init.sql
