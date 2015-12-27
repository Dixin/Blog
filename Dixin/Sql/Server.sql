-- Attach database.
USE master;
GO
CREATE DATABASE [Northwind] ON PRIMARY 
	(FILENAME = N'D:\SqlServer\SQL Server 2000 Sample Databases\NORTHWND.MDF')
LOG ON 
	(FILENAME = N'D:\SqlServer\SQL Server 2000 Sample Databases\NORTHWND.LDF')
FOR ATTACH;
GO

-- Attach database.
USE master;
GO

CREATE DATABASE [Northwind] ON 
	(FILENAME = N'D:\SqlServer\SQL Server 2000 Sample Databases\NORTHWND.MDF'), 
	(FILENAME = N'D:\SqlServer\SQL Server 2000 Sample Databases\NORTHWND.LDF') 
FOR ATTACH; 
GO

-- Detach database.
USE master;
GO

EXEC sys.sp_detach_db @dbname = N'Northwind', @skipchecks = N'true';
GO

-- Set database offline
ALTER DATABASE [AdventureWorks] SET OFFLINE WITH ROLLBACK IMMEDIATE;
GO

-- Set database online.
ALTER DATABASE [AdventureWorks] SET ONLINE;
GO

-- Change compatibility mode.
ALTER DATABASE [AdventureWorks]
SET COMPATIBILITY_LEVEL = 110;
GO

-- Query compatibility mode.
SELECT compatibility_level
FROM sys.databases WHERE name = N'AdventureWorks';
GO

-- Create server login.
CREATE LOGIN dixin 
	WITH PASSWORD = 'password';
GO

EXEC sys.sp_addsrvrolemember @loginame = N'dixin', @rolename = N'sysadmin';
Go
