-- Insert
BEGIN TRANSACTION
	exec sp_executesql N'INSERT [Production].[ProductCategory]([Name])
	VALUES (@0)
	SELECT [ProductCategoryID]
	FROM [Production].[ProductCategory]
	WHERE @@ROWCOUNT > 0 AND [ProductCategoryID] = scope_identity()',N'@0 nvarchar(50)',@0=N'Category'

	exec sp_executesql N'INSERT [Production].[ProductSubcategory]([ProductCategoryID], [Name])
	VALUES (@0, @1)
	SELECT [ProductSubcategoryID]
	FROM [Production].[ProductSubcategory]
	WHERE @@ROWCOUNT > 0 AND [ProductSubcategoryID] = scope_identity()',N'@0 int,@1 nvarchar(50)',@0=5,@1=N'Subcategory'
COMMIT TRANSACTION

-- Update
SELECT TOP (1) 
    [c].[ProductCategoryID] AS [ProductCategoryID], 
    [c].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [c]

SELECT TOP (2) 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductSubcategory] AS [Extent1]
    WHERE N'Subcategory' = [Extent1].[Name]

BEGIN TRANSACTION
	exec sp_executesql N'UPDATE [Production].[ProductSubcategory]
	SET [Name] = @0
	WHERE ([ProductSubcategoryID] = @1)
	',N'@0 nvarchar(50),@1 int',@0=N'Subcategory update',@1=38
COMMIT TRANSACTION

-- UpdateWithNoChange
SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[ProductID] AS [ProductID], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice]
    FROM ( SELECT TOP (2) 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE 999 = [Extent1].[ProductID]
    )  AS [Limit1]

-- Delete
SELECT TOP (2) 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE N'Category' = [Extent1].[Name]

BEGIN TRANSACTION
	exec sp_executesql N'DELETE [Production].[ProductCategory]
	WHERE ([ProductCategoryID] = @0)',N'@0 int',@0=8
COMMIT TRANSACTION

-- DeleteWithNoQuery
BEGIN TRANSACTION
	exec sp_executesql N'DELETE [Production].[ProductSubcategory]
	WHERE ([ProductSubcategoryID] = @0)',N'@0 int',@0=41
COMMIT TRANSACTION

-- DeleteWithAssociation
SELECT TOP (2) 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE N'Category' = [Extent1].[Name]

SELECT TOP (2) 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductSubcategory] AS [Extent1]
    WHERE N'Subcategory' = [Extent1].[Name]

BEGIN TRANSACTION
	exec sp_executesql N'DELETE [Production].[ProductSubcategory]
	WHERE ([ProductSubcategoryID] = @0)',N'@0 int',@0=39

	exec sp_executesql N'DELETE [Production].[ProductCategory]
	WHERE ([ProductCategoryID] = @0)',N'@0 int',@0=6
COMMIT TRANSACTION

-- Implicit
SELECT TOP (1) 
    [c].[ProductCategoryID] AS [ProductCategoryID], 
    [c].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [c]

SELECT TOP (1) 
    [c].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [c].[ProductCategoryID] AS [ProductCategoryID], 
    [c].[Name] AS [Name]
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

-- ExplicitLocal
BEGIN TRANSACTION
	exec sp_executesql N'INSERT [Production].[ProductCategory]([Name])
	VALUES (@0)
	SELECT [ProductCategoryID]
	FROM [Production].[ProductCategory]
	WHERE @@ROWCOUNT > 0 AND [ProductCategoryID] = scope_identity()',N'@0 nvarchar(50)',@0=N'Transaction'

	DELETE FROM [Production].[ProductCategory] WHERE [Name] = N'Transaction'
COMMIT TRANSACTION

-- ExplicitDistributable
BEGIN TRANSACTION
	INSERT INTO [Production].[ProductCategory] ([Name]) VALUES (N'Transaction')

	SELECT TOP (2) 
		[Extent1].[ProductCategoryID] AS [ProductCategoryID], 
		[Extent1].[Name] AS [Name]
		FROM [Production].[ProductCategory] AS [Extent1]
		WHERE N'Transaction' = [Extent1].[Name]

	exec sp_executesql N'DELETE [Production].[ProductCategory]
	WHERE ([ProductCategoryID] = @0)',N'@0 int',@0=22
COMMIT TRANSACTION

-- LastWins
BEGIN TRANSACTION
	exec sp_executesql N'SELECT TOP (2) 
		[Extent1].[ProductCategoryID] AS [ProductCategoryID], 
		[Extent1].[Name] AS [Name]
		FROM [Production].[ProductCategory] AS [Extent1]
		WHERE [Extent1].[ProductCategoryID] = @p0',N'@p0 int',@p0=1

	exec sp_executesql N'SELECT TOP (2) 
		[Extent1].[ProductCategoryID] AS [ProductCategoryID], 
		[Extent1].[Name] AS [Name]
		FROM [Production].[ProductCategory] AS [Extent1]
		WHERE [Extent1].[ProductCategoryID] = @p0',N'@p0 int',@p0=1

	exec sp_executesql N'UPDATE [Production].[ProductCategory]
	SET [Name] = @0
	WHERE ([ProductCategoryID] = @1)
	',N'@0 nvarchar(50),@1 int',@0=N'task1',@1=1

	exec sp_executesql N'UPDATE [Production].[ProductCategory]
	SET [Name] = @0
	WHERE ([ProductCategoryID] = @1)
	',N'@0 nvarchar(50),@1 int',@0=N'task2',@1=1

	exec sp_executesql N'SELECT TOP (2) 
		[Extent1].[ProductCategoryID] AS [ProductCategoryID], 
		[Extent1].[Name] AS [Name]
		FROM [Production].[ProductCategory] AS [Extent1]
		WHERE [Extent1].[ProductCategoryID] = @p0',N'@p0 int',@p0=1
ROLLBACK TRANSACTION

-- Check
BEGIN TRANSACTION
	exec sp_executesql N'SELECT TOP (2) 
		[Extent1].[ProductPhotoID] AS [ProductPhotoID], 
		[Extent1].[ModifiedDate] AS [ModifiedDate], 
		[Extent1].[LargePhotoFileName] AS [LargePhotoFileName]
		FROM [Production].[ProductPhoto] AS [Extent1]
		WHERE [Extent1].[ProductPhotoID] = @p0',N'@p0 int',@p0=1

	exec sp_executesql N'SELECT TOP (2) 
		[Extent1].[ProductPhotoID] AS [ProductPhotoID], 
		[Extent1].[ModifiedDate] AS [ModifiedDate], 
		[Extent1].[LargePhotoFileName] AS [LargePhotoFileName]
		FROM [Production].[ProductPhoto] AS [Extent1]
		WHERE [Extent1].[ProductPhotoID] = @p0',N'@p0 int',@p0=1

	exec sp_executesql N'UPDATE [Production].[ProductPhoto]
	SET [LargePhotoFileName] = @0
	WHERE (([ProductPhotoID] = @1) AND ([ModifiedDate] = @2))
	SELECT [ModifiedDate]
	FROM [Production].[ProductPhoto]
	WHERE @@ROWCOUNT > 0 AND [ProductPhotoID] = @1',N'@0 nvarchar(50),@1 int,@2 datetime2(7)',@0=N'task1',@1=1,@2='2008-04-30 00:00:00'

	exec sp_executesql N'UPDATE [Production].[ProductPhoto]
	SET [LargePhotoFileName] = @0
	WHERE (([ProductPhotoID] = @1) AND ([ModifiedDate] = @2))
	SELECT [ModifiedDate]
	FROM [Production].[ProductPhoto]
	WHERE @@ROWCOUNT > 0 AND [ProductPhotoID] = @1',N'@0 nvarchar(50),@1 int,@2 datetime2(7)',@0=N'task2',@1=1,@2='2008-04-30 00:00:00'
ROLLBACK TRANSACTION

-- DatabaseWins
BEGIN TRANSACTION
	exec sp_executesql N'SELECT 
		[Limit1].[C1] AS [C1], 
		[Limit1].[ProductID] AS [ProductID], 
		[Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
		[Limit1].[RowVersion] AS [RowVersion], 
		[Limit1].[Name] AS [Name], 
		[Limit1].[ListPrice] AS [ListPrice]
		FROM ( SELECT TOP (2) 
			[Extent1].[ProductID] AS [ProductID], 
			[Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
			[Extent1].[RowVersion] AS [RowVersion], 
			[Extent1].[Name] AS [Name], 
			[Extent1].[ListPrice] AS [ListPrice], 
			CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
			FROM [Production].[Product] AS [Extent1]
			WHERE [Extent1].[ProductID] = @p0
		)  AS [Limit1]',N'@p0 int',@p0=999

	exec sp_executesql N'SELECT 
		[Limit1].[C1] AS [C1], 
		[Limit1].[ProductID] AS [ProductID], 
		[Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
		[Limit1].[RowVersion] AS [RowVersion], 
		[Limit1].[Name] AS [Name], 
		[Limit1].[ListPrice] AS [ListPrice]
		FROM ( SELECT TOP (2) 
			[Extent1].[ProductID] AS [ProductID], 
			[Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
			[Extent1].[RowVersion] AS [RowVersion], 
			[Extent1].[Name] AS [Name], 
			[Extent1].[ListPrice] AS [ListPrice], 
			CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
			FROM [Production].[Product] AS [Extent1]
			WHERE [Extent1].[ProductID] = @p0
		)  AS [Limit1]',N'@p0 int',@p0=999

	exec sp_executesql N'UPDATE [Production].[Product]
	SET [Name] = @0, [ListPrice] = @1
	WHERE (([ProductID] = @2) AND ([RowVersion] = @3))
	SELECT [RowVersion]
	FROM [Production].[Product]
	WHERE @@ROWCOUNT > 0 AND [ProductID] = @2',N'@0 nvarchar(50),@1 decimal(18,2),@2 int,@3 binary(8)',@0=N'task1',@1=0,@2=999,@3=0x000000000000469F

	exec sp_executesql N'UPDATE [Production].[Product]
	SET [ProductSubcategoryID] = NULL, [Name] = @0
	WHERE (([ProductID] = @1) AND ([RowVersion] = @2))
	SELECT [RowVersion]
	FROM [Production].[Product]
	WHERE @@ROWCOUNT > 0 AND [ProductID] = @1',N'@0 nvarchar(50),@1 int,@2 binary(8)',@0=N'task2',@1=999,@2=0x000000000000469F

	SELECT 
		CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
		[Extent1].[ProductID] AS [ProductID], 
		[Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
		[Extent1].[RowVersion] AS [RowVersion], 
		[Extent1].[Name] AS [Name], 
		[Extent1].[ListPrice] AS [ListPrice]
		FROM [Production].[Product] AS [Extent1]
		WHERE [Extent1].[ProductID] = 999

	exec sp_executesql N'SELECT 
		[Limit1].[C1] AS [C1], 
		[Limit1].[ProductID] AS [ProductID], 
		[Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
		[Limit1].[RowVersion] AS [RowVersion], 
		[Limit1].[Name] AS [Name], 
		[Limit1].[ListPrice] AS [ListPrice]
		FROM ( SELECT TOP (2) 
			[Extent1].[ProductID] AS [ProductID], 
			[Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
			[Extent1].[RowVersion] AS [RowVersion], 
			[Extent1].[Name] AS [Name], 
			[Extent1].[ListPrice] AS [ListPrice], 
			CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
			FROM [Production].[Product] AS [Extent1]
			WHERE [Extent1].[ProductID] = @p0
		)  AS [Limit1]',N'@p0 int',@p0=999
ROLLBACK TRANSACTION

-- ClientWins
BEGIN TRANSACTION
	exec sp_executesql N'SELECT 
		[Limit1].[C1] AS [C1], 
		[Limit1].[ProductID] AS [ProductID], 
		[Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
		[Limit1].[RowVersion] AS [RowVersion], 
		[Limit1].[Name] AS [Name], 
		[Limit1].[ListPrice] AS [ListPrice]
		FROM ( SELECT TOP (2) 
			[Extent1].[ProductID] AS [ProductID], 
			[Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
			[Extent1].[RowVersion] AS [RowVersion], 
			[Extent1].[Name] AS [Name], 
			[Extent1].[ListPrice] AS [ListPrice], 
			CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
			FROM [Production].[Product] AS [Extent1]
			WHERE [Extent1].[ProductID] = @p0
		)  AS [Limit1]',N'@p0 int',@p0=999

	exec sp_executesql N'SELECT 
		[Limit1].[C1] AS [C1], 
		[Limit1].[ProductID] AS [ProductID], 
		[Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
		[Limit1].[RowVersion] AS [RowVersion], 
		[Limit1].[Name] AS [Name], 
		[Limit1].[ListPrice] AS [ListPrice]
		FROM ( SELECT TOP (2) 
			[Extent1].[ProductID] AS [ProductID], 
			[Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
			[Extent1].[RowVersion] AS [RowVersion], 
			[Extent1].[Name] AS [Name], 
			[Extent1].[ListPrice] AS [ListPrice], 
			CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
			FROM [Production].[Product] AS [Extent1]
			WHERE [Extent1].[ProductID] = @p0
		)  AS [Limit1]',N'@p0 int',@p0=999

	exec sp_executesql N'UPDATE [Production].[Product]
	SET [Name] = @0, [ListPrice] = @1
	WHERE (([ProductID] = @2) AND ([RowVersion] = @3))
	SELECT [RowVersion]
	FROM [Production].[Product]
	WHERE @@ROWCOUNT > 0 AND [ProductID] = @2',N'@0 nvarchar(50),@1 decimal(18,2),@2 int,@3 binary(8)',@0=N'task1',@1=0,@2=999,@3=0x000000000000469F

	exec sp_executesql N'UPDATE [Production].[Product]
	SET [ProductSubcategoryID] = NULL, [Name] = @0
	WHERE (([ProductID] = @1) AND ([RowVersion] = @2))
	SELECT [RowVersion]
	FROM [Production].[Product]
	WHERE @@ROWCOUNT > 0 AND [ProductID] = @1',N'@0 nvarchar(50),@1 int,@2 binary(8)',@0=N'task2',@1=999,@2=0x000000000000469F

	exec sp_executesql N'SELECT 
		[Limit1].[ProductID] AS [ProductID], 
		[Limit1].[C1] AS [C1], 
		[Limit1].[C2] AS [C2], 
		[Limit1].[C3] AS [C3], 
		[Limit1].[C4] AS [C4], 
		[Limit1].[C5] AS [C5]
		FROM ( SELECT TOP (2) 
			[Extent1].[ProductID] AS [ProductID], 
			CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X1X%'') THEN [Extent1].[ProductID] END AS [C1], 
			CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X1X%'') THEN [Extent1].[ProductSubcategoryID] END AS [C2], 
			CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X1X%'') THEN [Extent1].[RowVersion] END AS [C3], 
			CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X1X%'') THEN [Extent1].[Name] END AS [C4], 
			CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X1X%'') THEN [Extent1].[ListPrice] END AS [C5]
			FROM [Production].[Product] AS [Extent1]
			WHERE ((CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X1X%'') THEN [Extent1].[ProductID] END) = @p0) OR ((CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X1X%'') THEN [Extent1].[ProductID] END IS NULL) AND (@p0 IS NULL))
		)  AS [Limit1]',N'@p0 int',@p0=999

	exec sp_executesql N'UPDATE [Production].[Product]
	SET [ProductSubcategoryID] = NULL, [Name] = @0, [ListPrice] = @1
	WHERE (([ProductID] = @2) AND ([RowVersion] = @3))
	SELECT [RowVersion]
	FROM [Production].[Product]
	WHERE @@ROWCOUNT > 0 AND [ProductID] = @2',N'@0 nvarchar(50),@1 decimal(18,2),@2 int,@3 binary(8)',@0=N'task2',@1=539.99,@2=999,@3=0x000000000001BD58

	exec sp_executesql N'SELECT 
		[Limit1].[C1] AS [C1], 
		[Limit1].[ProductID] AS [ProductID], 
		[Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
		[Limit1].[RowVersion] AS [RowVersion], 
		[Limit1].[Name] AS [Name], 
		[Limit1].[ListPrice] AS [ListPrice]
		FROM ( SELECT TOP (2) 
			[Extent1].[ProductID] AS [ProductID], 
			[Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
			[Extent1].[RowVersion] AS [RowVersion], 
			[Extent1].[Name] AS [Name], 
			[Extent1].[ListPrice] AS [ListPrice], 
			CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
			FROM [Production].[Product] AS [Extent1]
			WHERE [Extent1].[ProductID] = @p0
		)  AS [Limit1]',N'@p0 int',@p0=999
ROLLBACK TRANSACTION

-- MergeClientAndDatabase
BEGIN TRANSACTION
	exec sp_executesql N'SELECT 
		[Limit1].[C1] AS [C1], 
		[Limit1].[ProductID] AS [ProductID], 
		[Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
		[Limit1].[RowVersion] AS [RowVersion], 
		[Limit1].[Name] AS [Name], 
		[Limit1].[ListPrice] AS [ListPrice]
		FROM ( SELECT TOP (2) 
			[Extent1].[ProductID] AS [ProductID], 
			[Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
			[Extent1].[RowVersion] AS [RowVersion], 
			[Extent1].[Name] AS [Name], 
			[Extent1].[ListPrice] AS [ListPrice], 
			CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
			FROM [Production].[Product] AS [Extent1]
			WHERE [Extent1].[ProductID] = @p0
		)  AS [Limit1]',N'@p0 int',@p0=999

	exec sp_executesql N'SELECT 
		[Limit1].[C1] AS [C1], 
		[Limit1].[ProductID] AS [ProductID], 
		[Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
		[Limit1].[RowVersion] AS [RowVersion], 
		[Limit1].[Name] AS [Name], 
		[Limit1].[ListPrice] AS [ListPrice]
		FROM ( SELECT TOP (2) 
			[Extent1].[ProductID] AS [ProductID], 
			[Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
			[Extent1].[RowVersion] AS [RowVersion], 
			[Extent1].[Name] AS [Name], 
			[Extent1].[ListPrice] AS [ListPrice], 
			CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
			FROM [Production].[Product] AS [Extent1]
			WHERE [Extent1].[ProductID] = @p0
		)  AS [Limit1]',N'@p0 int',@p0=999

	exec sp_executesql N'UPDATE [Production].[Product]
	SET [Name] = @0, [ListPrice] = @1
	WHERE (([ProductID] = @2) AND ([RowVersion] = @3))
	SELECT [RowVersion]
	FROM [Production].[Product]
	WHERE @@ROWCOUNT > 0 AND [ProductID] = @2',N'@0 nvarchar(50),@1 decimal(18,2),@2 int,@3 binary(8)',@0=N'task1',@1=0,@2=999,@3=0x000000000000469F

	exec sp_executesql N'UPDATE [Production].[Product]
	SET [ProductSubcategoryID] = NULL, [Name] = @0
	WHERE (([ProductID] = @1) AND ([RowVersion] = @2))
	SELECT [RowVersion]
	FROM [Production].[Product]
	WHERE @@ROWCOUNT > 0 AND [ProductID] = @1',N'@0 nvarchar(50),@1 int,@2 binary(8)',@0=N'task2',@1=999,@2=0x000000000000469F

	exec sp_executesql N'SELECT 
		[Limit1].[ProductID] AS [ProductID], 
		[Limit1].[C1] AS [C1], 
		[Limit1].[C2] AS [C2], 
		[Limit1].[C3] AS [C3], 
		[Limit1].[C4] AS [C4], 
		[Limit1].[C5] AS [C5]
		FROM ( SELECT TOP (2) 
			[Extent1].[ProductID] AS [ProductID], 
			CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X1X%'') THEN [Extent1].[ProductID] END AS [C1], 
			CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X1X%'') THEN [Extent1].[ProductSubcategoryID] END AS [C2], 
			CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X1X%'') THEN [Extent1].[RowVersion] END AS [C3], 
			CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X1X%'') THEN [Extent1].[Name] END AS [C4], 
			CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X1X%'') THEN [Extent1].[ListPrice] END AS [C5]
			FROM [Production].[Product] AS [Extent1]
			WHERE ((CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X1X%'') THEN [Extent1].[ProductID] END) = @p0) OR ((CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X1X%'') THEN [Extent1].[ProductID] END IS NULL) AND (@p0 IS NULL))
		)  AS [Limit1]',N'@p0 int',@p0=999

	exec sp_executesql N'UPDATE [Production].[Product]
	SET [ProductSubcategoryID] = NULL, [Name] = @0, [ListPrice] = @1
	WHERE (([ProductID] = @2) AND ([RowVersion] = @3))
	SELECT [RowVersion]
	FROM [Production].[Product]
	WHERE @@ROWCOUNT > 0 AND [ProductID] = @2',N'@0 nvarchar(50),@1 decimal(18,2),@2 int,@3 binary(8)',@0=N'task1',@1=0,@2=999,@3=0x000000000001BD5A

	exec sp_executesql N'SELECT 
		[Limit1].[C1] AS [C1], 
		[Limit1].[ProductID] AS [ProductID], 
		[Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
		[Limit1].[RowVersion] AS [RowVersion], 
		[Limit1].[Name] AS [Name], 
		[Limit1].[ListPrice] AS [ListPrice]
		FROM ( SELECT TOP (2) 
			[Extent1].[ProductID] AS [ProductID], 
			[Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
			[Extent1].[RowVersion] AS [RowVersion], 
			[Extent1].[Name] AS [Name], 
			[Extent1].[ListPrice] AS [ListPrice], 
			CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
			FROM [Production].[Product] AS [Extent1]
			WHERE [Extent1].[ProductID] = @p0
		)  AS [Limit1]',N'@p0 int',@p0=999
ROLLBACK TRANSACTION
