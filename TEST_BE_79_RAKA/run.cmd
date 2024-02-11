echo off
sqlcmd -S tcp:localhost,3503 -i Data\init.sql
dotnet run