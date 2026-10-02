@echo off
set SQLCMD="C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE"
%SQLCMD% -S . -E -d ClientNew -i "E:\MohSaleh\MyWorkAll\SIT_Projects .Net\SIT_Projects\Razors\AccountERP.Web\AccountERP.Web\add_test_cost_centers.sql"
pause