-- Create
BEGIN TRANSACTION
    exec sp_executesql N'INSERT [Production].[ProductCategory]([Name])
    VALUES (@0)
    SELECT [ProductCategoryID]
    FROM [Production].[ProductCategory]
    WHERE @@ROWCOUNT > 0 AND [ProductCategoryID] = scope_identity()',N'@0 nvarchar(50)',@0=N'ProductCategory'

    exec sp_executesql N'INSERT [Production].[ProductSubcategory]([Name], [ProductCategoryID])
    VALUES (@0, @1)
    SELECT [ProductSubcategoryID]
    FROM [Production].[ProductSubcategory]
    WHERE @@ROWCOUNT > 0 AND [ProductSubcategoryID] = scope_identity()',N'@0 nvarchar(50),@1 int',@0=N'ProductSubcategory',@1=25
COMMIT TRANSACTION

-- Update
SELECT TOP (2) 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE N'Bikes' = [Extent1].[Name]

SELECT TOP (2) 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID]
    FROM [Production].[ProductSubcategory] AS [Extent1]
    WHERE N'ProductSubcategory' = [Extent1].[Name]

BEGIN TRANSACTION
    exec sp_executesql N'UPDATE [Production].[ProductSubcategory]
    SET [Name] = @0, [ProductCategoryID] = @1
    WHERE ([ProductSubcategoryID] = @2)
    ',N'@0 nvarchar(50),@1 int,@2 int',@0=N'Update',@1=1,@2=50
COMMIT TRANSACTION

-- UpdateWithoutRead
BEGIN TRANSACTION
    exec sp_executesql N'UPDATE [Production].[ProductCategory]
    SET [Name] = @0
    WHERE ([ProductCategoryID] = @1)
    ',N'@0 nvarchar(50),@1 int',@0=N'f20d6c0c-1e92-4060-8a5d-72c41062b1be',@1=25
BEGIN TRANSACTION

-- SaveNoChanges
exec sp_executesql N'SELECT TOP (2) 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE [Extent1].[ProductCategoryID] = @p0',N'@p0 int',@p0=1

-- Delete
SELECT TOP (1) 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID]
    FROM [Production].[ProductSubcategory] AS [Extent1]
    ORDER BY [Extent1].[ProductSubcategoryID] DESC

BEGIN TRANSACTION
    exec sp_executesql N'DELETE [Production].[ProductSubcategory]
    WHERE ([ProductSubcategoryID] = @0)',N'@0 int',@0=50
COMMIT TRANSACTION

-- DeleteWithoutRead
BEGIN TRANSACTION
    exec sp_executesql N'DELETE [Production].[ProductCategory]
    WHERE ([ProductCategoryID] = @0)',N'@0 int',@0=25
COMMIT TRANSACTION

-- DeleteWithAssociation
exec sp_executesql N'SELECT TOP (2) 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE [Extent1].[ProductCategoryID] = @p0',N'@p0 int',@p0=1

BEGIN TRANSACTION
    exec sp_executesql N'DELETE [Production].[ProductCategory]
    WHERE ([ProductCategoryID] = @0)',N'@0 int',@0=1036
ROLLBACK TRANSACTION

-- DeleteAllAssociated
SELECT TOP (2) 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE N'ProductCategory' = [Extent1].[Name]

exec sp_executesql N'SELECT 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID]
    FROM [Production].[ProductSubcategory] AS [Extent1]
    WHERE [Extent1].[ProductCategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=26

BEGIN TRANSACTION
    exec sp_executesql N'DELETE [Production].[ProductSubcategory]
    WHERE ([ProductSubcategoryID] = @0)',N'@0 int',@0=51

    exec sp_executesql N'DELETE [Production].[ProductCategory]
    WHERE ([ProductCategoryID] = @0)',N'@0 int',@0=26
COMMIT TRANSACTION

-- UntrackedChanges
SELECT TOP (1) 
    [c].[ProductCategoryID] AS [ProductCategoryID], 
    [c].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [c]

-- Default
SELECT TOP (1) 
    [c].[ProductCategoryID] AS [ProductCategoryID], 
    [c].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [c]

SELECT TOP (1) 
    [c].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [c].[Name] AS [Name], 
    [c].[ProductCategoryID] AS [ProductCategoryID]
    FROM [Production].[ProductSubcategory] AS [c]

BEGIN TRANSACTION
    exec sp_executesql N'UPDATE [Production].[ProductCategory]
    SET [Name] = @0
    WHERE ([ProductCategoryID] = @1)
    ',N'@0 nvarchar(50),@1 int',@0=N'Update',@1=4

    exec sp_executesql N'UPDATE [Production].[ProductSubcategory]
    SET [ProductCategoryID] = @0
    WHERE ([ProductSubcategoryID] = @1)
    ',N'@0 int,@1 int',@0=-1,@1=1
ROLLBACK TRANSACTION

SELECT TOP (1) 
    [c].[ProductCategoryID] AS [ProductCategoryID], 
    [c].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [c]

SELECT TOP (1) 
    [c].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [c].[ProductCategoryID] AS [ProductCategoryID], 
    [c].[Name] AS [Name]
    FROM [Production].[ProductSubcategory] AS [c]

-- DbContextTransaction
BEGIN TRANSACTION
    SELECT         
        CASE transaction_isolation_level
            WHEN 0 THEN N'Unspecified'
            WHEN 1 THEN N'ReadUncommitted'
            WHEN 2 THEN N'ReadCommitted'
            WHEN 3 THEN N'RepeatableRead'
            WHEN 4 THEN N'Serializable'
            WHEN 5 THEN N'Snapshot'
        END
    FROM sys.dm_exec_sessions
    WHERE session_id = @@SPID

    exec sp_executesql N'INSERT [Production].[ProductCategory]([Name])
    VALUES (@0)
    SELECT [ProductCategoryID]
    FROM [Production].[ProductCategory]
    WHERE @@ROWCOUNT > 0 AND [ProductCategoryID] = scope_identity()',N'@0 nvarchar(50)',@0=N'ProductCategory'

    exec sp_executesql N'DELETE FROM [Production].[ProductCategory] WHERE [Name] = @p0',N'@p0 nvarchar(15)',@p0=N'ProductCategory'
COMMIT TRANSACTION

-- DbTransaction
BEGIN TRANSACTION
    SELECT         
        CASE transaction_isolation_level
            WHEN 0 THEN N'Unspecified'
            WHEN 1 THEN N'ReadUncommitted'
            WHEN 2 THEN N'ReadCommitted'
            WHEN 3 THEN N'RepeatableRead'
            WHEN 4 THEN N'Serializable'
            WHEN 5 THEN N'Snapshot'
        END
    FROM sys.dm_exec_sessions
    WHERE session_id = @@SPID

    exec sp_executesql N'INSERT [Production].[ProductCategory]([Name])
    VALUES (@0)
    SELECT [ProductCategoryID]
    FROM [Production].[ProductCategory]
    WHERE @@ROWCOUNT > 0 AND [ProductCategoryID] = scope_identity()',N'@0 nvarchar(50)',@0=N'ProductCategory'

    exec sp_executesql N'DELETE FROM [Production].[ProductCategory] WHERE [Name] = @p0',N'@p0 nvarchar(15)',@p0=N'ProductCategory'
COMMIT TRANSACTION 

-- TransactionScope
BEGIN TRANSACTION
    SELECT         
        CASE transaction_isolation_level
            WHEN 0 THEN N'Unspecified'
            WHEN 1 THEN N'ReadUncommitted'
            WHEN 2 THEN N'ReadCommitted'
            WHEN 3 THEN N'RepeatableRead'
            WHEN 4 THEN N'Serializable'
            WHEN 5 THEN N'Snapshot'
        END
    FROM sys.dm_exec_sessions
    WHERE session_id = @@SPID

    exec sp_executesql N'INSERT [Production].[ProductCategory]([Name])
    VALUES (@0)
    SELECT [ProductCategoryID]
    FROM [Production].[ProductCategory]
    WHERE @@ROWCOUNT > 0 AND [ProductCategoryID] = scope_identity()',N'@0 nvarchar(50)',@0=N'ProductCategory'

    exec sp_executesql N'DELETE FROM [Production].[ProductCategory] WHERE [Name] = @p0',N'@p0 nvarchar(15)',@p0=N'ProductCategory'
COMMIT TRANSACTION
