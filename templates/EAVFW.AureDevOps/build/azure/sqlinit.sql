CREATE USER [$(WebAppName)] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [$(WebAppName)];
ALTER ROLE db_datawriter ADD MEMBER [$(WebAppName)];
ALTER ROLE db_ddladmin ADD MEMBER [$(WebAppName)];
GO
EXECUTE AS [$(WebAppName)]
GO
CREATE SCHEMA [$(schema)]
GO
REVERT
