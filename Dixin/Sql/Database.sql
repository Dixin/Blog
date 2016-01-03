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
	
-- Query table-valued function output schema.
SELECT *
FROM sys.columns
WHERE object_id = OBJECT_ID(N'dbo.ufnGetContactInformation')

-- Query stored procedure first output schema.
SELECT *
FROM sys.dm_exec_describe_first_result_set_for_object(OBJECT_ID(N'dbo.uspGetManagerEmployees'), 0);

SELECT *
FROM sys.dm_exec_describe_first_result_set(N'dbo.uspGetManagerEmployees', NULL, 0);

-- Query T-SQL first output schema.
SELECT *
FROM sys.dm_exec_describe_first_result_set(
	N'SELECT object_id, name, type_desc FROM sys.indexes', NULL, 0);

SELECT * 
FROM sys.dm_exec_describe_first_result_set(
	N'SELECT CustomerID, TerritoryID, AccountNumber FROM Sales.Customer WHERE CustomerID = @CustomerID;
	SELECT * FROM Sales.SalesOrderHeader;',
	N'@CustomerID int',
	0);

-- Query table-valued function result.
SELECT *
FROM sys.columns
WHERE object_id = object_id('dbo.ufnGetContactInformation');

-- Enable CLR.
EXEC sys.sp_configure @configname = N'clr enabled', @configvalue = 1;
GO

RECONFIGURE;
GO

-- Drop aggregate.
DROP AGGREGATE [dbo].[Concat];
GO

DROP AGGREGATE [dbo].[ConcatWith];
GO

-- Drop assembly.
DROP ASSEMBLY [Dixin.Sql];
GO

-- Table space usage
sys.sp_msforeachtable N'sys.sp_spaceused [?]'
