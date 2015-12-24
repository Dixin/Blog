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
EXEC sp_detach_db @dbname = N'Northwind', @skipchecks = N'true';
GO

-- Query all foreign keys.
SELECT 
	fk.name AS ForeignKey,
	OBJECT_SCHEMA_NAME(fk.parent_object_id) AS TableNameSchema,
	OBJECT_NAME(fk.parent_object_id) AS TableName,
	COL_NAME(col.parent_object_id, col.parent_column_id) AS ColumnName,
	OBJECT_SCHEMA_NAME(fk.referenced_object_id) AS ReferenceTableNameSchema,
	OBJECT_NAME (fk.referenced_object_id) AS ReferenceTableName,
	COL_NAME(col.referenced_object_id, col.referenced_column_id) AS ReferenceColumnName
FROM
	sys.foreign_keys AS fk
	INNER JOIN sys.foreign_key_columns AS col ON fk.object_id = col.constraint_object_id
	INNER JOIN sys.objects ON sys.objects.object_id = col.referenced_object_id;

-- Change compatibility mode.
ALTER DATABASE [AdventureWorks2014]
SET COMPATIBILITY_LEVEL = 110;
GO

-- Query compatibility mode.
SELECT compatibility_level
FROM sys.databases WHERE name = N'AdventureWorks2014';
GO

-- Query table-valued function result.
SELECT *
FROM sys.columns
WHERE object_id = object_id('dbo.ufnGetContactInformation')
