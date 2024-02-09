echo off
sqlcmd -i Data\init.sql
dotnet run