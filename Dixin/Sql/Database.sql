-- Query all foreign keys.
SELECT 
	foreign_keys.name AS ForeignKey,
	OBJECT_SCHEMA_NAME(foreign_keys.parent_object_id) AS TableSchema,
	OBJECT_NAME(foreign_keys.parent_object_id) AS TableName,
	COL_NAME(columns.parent_object_id, columns.parent_column_id) AS ColumnName,
	OBJECT_SCHEMA_NAME(foreign_keys.referenced_object_id) AS ReferenceTableSchema,
	OBJECT_NAME (foreign_keys.referenced_object_id) AS ReferenceTableName,
	COL_NAME(columns.referenced_object_id, columns.referenced_column_id) AS ReferenceColumnName
FROM
	sys.foreign_keys AS foreign_keys
	INNER JOIN sys.foreign_key_columns AS columns ON foreign_keys.object_id = columns.constraint_object_id
	INNER JOIN sys.objects AS objects ON objects.object_id = columns.referenced_object_id;

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

-- QUery table space usage.
sys.sp_msforeachtable N'sys.sp_spaceused [?]'

-- Query table triggers.
SELECT
    schemas.name AS [TableSchema],
    OBJECT_NAME(parent_obj) AS [TableName],
	USER_NAME(sysobjects.uid) AS [TriggerOwner],
	sysobjects.name AS [TriggerName],
    OBJECTPROPERTY(id, N'ExecIsUpdateTrigger') AS [Update],
    OBJECTPROPERTY(id, N'ExecIsDeleteTrigger') AS [Delete],
    OBJECTPROPERTY(id, N'ExecIsInsertTrigger') AS [Insert],
    OBJECTPROPERTY(id, N'ExecIsAfterTrigger') AS [After],
    OBJECTPROPERTY(id, N'ExecIsInsteadOfTrigger') AS [InsteadOf],
    OBJECTPROPERTY(id, N'ExecIsTriggerDisabled') AS [Disabled]
FROM sys.sysobjects
INNER JOIN sys.tables
    ON sysobjects.parent_obj = tables.object_id
INNER JOIN sys.schemas
    ON tables.schema_id = schemas.schema_id
WHERE sysobjects.type = N'TR'