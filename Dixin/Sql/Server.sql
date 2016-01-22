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
ALTER DATABASE [D:\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF]
	SET OFFLINE WITH ROLLBACK IMMEDIATE;
GO

-- Set database online.
ALTER DATABASE [D:\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF]
	SET ONLINE;
GO

-- Change compatibility mode.
ALTER DATABASE [D:\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF]
	SET COMPATIBILITY_LEVEL = 110;
GO

-- Query compatibility mode.
SELECT compatibility_level FROM sys.databases WHERE name = N'D:\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF';
GO

-- Create server login.
CREATE LOGIN dixin 
	WITH PASSWORD = N'password';
GO

EXEC sys.sp_addsrvrolemember @loginame = N'dixin', @rolename = N'sysadmin';
Go

-- Query all databases' files.
SELECT
	databases.name,
	master_files.name,
	master_files.physical_name,
	master_files.type_desc,
	master_files.state_desc
FROM sys.master_files
INNER JOIN sys.databases
	ON master_files.database_id = databases.database_id;

-- Query databases' recovery mode.
SELECT name, recovery_model_desc FROM sys.databases