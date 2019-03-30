-- NoCheck
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
',N'@0 nvarchar(50),@1 int',@0=N'readerWriter1',@1=1

exec sp_executesql N'UPDATE [Production].[ProductCategory]
SET [Name] = @0
WHERE ([ProductCategoryID] = @1)
',N'@0 nvarchar(50),@1 int',@0=N'readerWriter2',@1=1

exec sp_executesql N'SELECT TOP (2) 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE [Extent1].[ProductCategoryID] = @p0',N'@p0 int',@p0=1

exec sp_executesql N'UPDATE [Production].[ProductCategory]
SET [Name] = @0
WHERE ([ProductCategoryID] = @1)
',N'@0 nvarchar(50),@1 int',@0=N'Bikes',@1=1

-- ConcurrencyCheck
exec sp_executesql N'SELECT TOP (2) 
    [Extent1].[ProductPhotoID] AS [ProductPhotoID], 
    [Extent1].[LargePhotoFileName] AS [LargePhotoFileName], 
    [Extent1].[ModifiedDate] AS [ModifiedDate]
    FROM [Production].[ProductPhoto] AS [Extent1]
    WHERE [Extent1].[ProductPhotoID] = @p0',N'@p0 int',@p0=1

exec sp_executesql N'SELECT TOP (2) 
    [Extent1].[ProductPhotoID] AS [ProductPhotoID], 
    [Extent1].[LargePhotoFileName] AS [LargePhotoFileName], 
    [Extent1].[ModifiedDate] AS [ModifiedDate]
    FROM [Production].[ProductPhoto] AS [Extent1]
    WHERE [Extent1].[ProductPhotoID] = @p0',N'@p0 int',@p0=1

exec sp_executesql N'UPDATE [Production].[ProductPhoto]
SET [LargePhotoFileName] = @0, [ModifiedDate] = @1
WHERE (([ProductPhotoID] = @2) AND ([ModifiedDate] = @3))
',N'@0 nvarchar(50),@1 datetime2(7),@2 int,@3 datetime2(7)',@0=N'readerWriter1',@1='2016-07-04 23:24:24.6053455',@2=1,@3='2008-04-30 00:00:00'

exec sp_executesql N'UPDATE [Production].[ProductPhoto]
SET [LargePhotoFileName] = @0, [ModifiedDate] = @1
WHERE (([ProductPhotoID] = @2) AND ([ModifiedDate] = @3))
',N'@0 nvarchar(50),@1 datetime2(7),@2 int,@3 datetime2(7)',@0=N'readerWriter1',@1='2016-07-04 23:24:24.6293420',@2=1,@3='2008-04-30 00:00:00'

-- RowVersion
exec sp_executesql N'SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[ProductID] AS [ProductID], 
    [Limit1].[RowVersion] AS [RowVersion], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM ( SELECT TOP (2) 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[RowVersion] AS [RowVersion], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ProductID] = @p0
    )  AS [Limit1]',N'@p0 int',@p0=999

exec sp_executesql N'SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[ProductID] AS [ProductID], 
    [Limit1].[RowVersion] AS [RowVersion], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM ( SELECT TOP (2) 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[RowVersion] AS [RowVersion], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ProductID] = @p0
    )  AS [Limit1]',N'@p0 int',@p0=999

exec sp_executesql N'UPDATE [Production].[Product]
SET [Name] = @0
WHERE (([ProductID] = @1) AND ([RowVersion] = @2))
SELECT [RowVersion]
FROM [Production].[Product]
WHERE @@ROWCOUNT > 0 AND [ProductID] = @1',N'@0 nvarchar(50),@1 int,@2 binary(8)',@0=N'readerWriter1',@1=999,@2=0x0000000000000803

exec sp_executesql N'DELETE [Production].[Product]
WHERE (([ProductID] = @0) AND ([RowVersion] = @1))',N'@0 int,@1 binary(8)',@0=999,@1=0x0000000000000803

-- DatabaseWins
exec sp_executesql N'SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[ProductID] AS [ProductID], 
    [Limit1].[RowVersion] AS [RowVersion], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM ( SELECT TOP (2) 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[RowVersion] AS [RowVersion], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ProductID] = @p0
    )  AS [Limit1]',N'@p0 int',@p0=999

exec sp_executesql N'SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[ProductID] AS [ProductID], 
    [Limit1].[RowVersion] AS [RowVersion], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM ( SELECT TOP (2) 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[RowVersion] AS [RowVersion], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ProductID] = @p0
    )  AS [Limit1]',N'@p0 int',@p0=999

exec sp_executesql N'UPDATE [Production].[Product]
SET [Name] = @0, [ListPrice] = @1
WHERE (([ProductID] = @2) AND ([RowVersion] = @3))
SELECT [RowVersion]
FROM [Production].[Product]
WHERE @@ROWCOUNT > 0 AND [ProductID] = @2',N'@0 nvarchar(50),@1 decimal(18,2),@2 int,@3 binary(8)',@0=N'readerWriter1',@1=100.00,@2=999,@3=0x00000000000007D1

exec sp_executesql N'UPDATE [Production].[Product]
SET [Name] = @0, [ProductSubcategoryID] = @1
WHERE (([ProductID] = @2) AND ([RowVersion] = @3))
SELECT [RowVersion]
FROM [Production].[Product]
WHERE @@ROWCOUNT > 0 AND [ProductID] = @2',N'@0 nvarchar(50),@1 int,@2 int,@3 binary(8)',@0=N'readerWriter2',@1=1,@2=999,@3=0x00000000000007D1

SELECT 
    CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M') THEN '0X0X' WHEN ([Extent1].[Style] = N'U') THEN '0X1X' ELSE '0X2X' END AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[RowVersion] AS [RowVersion], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[ProductID] = 999

exec sp_executesql N'SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[ProductID] AS [ProductID], 
    [Limit1].[RowVersion] AS [RowVersion], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM ( SELECT TOP (2) 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[RowVersion] AS [RowVersion], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ProductID] = @p0
    )  AS [Limit1]',N'@p0 int',@p0=999

-- ClientWins
exec sp_executesql N'SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[ProductID] AS [ProductID], 
    [Limit1].[RowVersion] AS [RowVersion], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM ( SELECT TOP (2) 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[RowVersion] AS [RowVersion], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ProductID] = @p0
    )  AS [Limit1]',N'@p0 int',@p0=999

exec sp_executesql N'SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[ProductID] AS [ProductID], 
    [Limit1].[RowVersion] AS [RowVersion], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM ( SELECT TOP (2) 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[RowVersion] AS [RowVersion], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ProductID] = @p0
    )  AS [Limit1]',N'@p0 int',@p0=999

exec sp_executesql N'UPDATE [Production].[Product]
SET [Name] = @0, [ListPrice] = @1
WHERE (([ProductID] = @2) AND ([RowVersion] = @3))
SELECT [RowVersion]
FROM [Production].[Product]
WHERE @@ROWCOUNT > 0 AND [ProductID] = @2',N'@0 nvarchar(50),@1 decimal(18,2),@2 int,@3 binary(8)',@0=N'readerWriter1',@1=100.00,@2=999,@3=0x00000000000007D1

exec sp_executesql N'UPDATE [Production].[Product]
SET [Name] = @0, [ProductSubcategoryID] = @1
WHERE (([ProductID] = @2) AND ([RowVersion] = @3))
SELECT [RowVersion]
FROM [Production].[Product]
WHERE @@ROWCOUNT > 0 AND [ProductID] = @2',N'@0 nvarchar(50),@1 int,@2 int,@3 binary(8)',@0=N'readerWriter2',@1=1,@2=999,@3=0x00000000000007D1

exec sp_executesql N'SELECT 
    [Limit1].[ProductID] AS [ProductID], 
    [Limit1].[C1] AS [C1], 
    [Limit1].[C2] AS [C2], 
    [Limit1].[C3] AS [C3], 
    [Limit1].[C4] AS [C4], 
    [Limit1].[C5] AS [C5]
    FROM ( SELECT TOP (2) 
        [Extent1].[ProductID] AS [ProductID], 
        CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X%'') THEN [Extent1].[ProductID] END AS [C1], 
        CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X%'') THEN [Extent1].[RowVersion] END AS [C2], 
        CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X%'') THEN [Extent1].[Name] END AS [C3], 
        CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X%'') THEN [Extent1].[ListPrice] END AS [C4], 
        CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X%'') THEN [Extent1].[ProductSubcategoryID] END AS [C5]
        FROM [Production].[Product] AS [Extent1]
        WHERE ((CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X%'') THEN [Extent1].[ProductID] END) = @p0) OR ((CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X%'') THEN [Extent1].[ProductID] END IS NULL) AND (@p0 IS NULL))
    )  AS [Limit1]',N'@p0 int',@p0=999

exec sp_executesql N'UPDATE [Production].[Product]
SET [Name] = @0, [ListPrice] = @1, [ProductSubcategoryID] = @2
WHERE (([ProductID] = @3) AND ([RowVersion] = @4))
SELECT [RowVersion]
FROM [Production].[Product]
WHERE @@ROWCOUNT > 0 AND [ProductID] = @3',N'@0 nvarchar(50),@1 decimal(18,2),@2 int,@3 int,@4 binary(8)',@0=N'readerWriter2',@1=256.49,@2=1,@3=999,@4=0x0000000000036336

exec sp_executesql N'SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[ProductID] AS [ProductID], 
    [Limit1].[RowVersion] AS [RowVersion], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM ( SELECT TOP (2) 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[RowVersion] AS [RowVersion], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ProductID] = @p0
    )  AS [Limit1]',N'@p0 int',@p0=999

-- MergeClientAndDatabase
exec sp_executesql N'SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[ProductID] AS [ProductID], 
    [Limit1].[RowVersion] AS [RowVersion], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM ( SELECT TOP (2) 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[RowVersion] AS [RowVersion], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ProductID] = @p0
    )  AS [Limit1]',N'@p0 int',@p0=999

exec sp_executesql N'SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[ProductID] AS [ProductID], 
    [Limit1].[RowVersion] AS [RowVersion], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM ( SELECT TOP (2) 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[RowVersion] AS [RowVersion], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ProductID] = @p0
    )  AS [Limit1]',N'@p0 int',@p0=999

exec sp_executesql N'UPDATE [Production].[Product]
SET [Name] = @0, [ListPrice] = @1
WHERE (([ProductID] = @2) AND ([RowVersion] = @3))
SELECT [RowVersion]
FROM [Production].[Product]
WHERE @@ROWCOUNT > 0 AND [ProductID] = @2',N'@0 nvarchar(50),@1 decimal(18,2),@2 int,@3 binary(8)',@0=N'readerWriter1',@1=100.00,@2=999,@3=0x00000000000007D1

exec sp_executesql N'UPDATE [Production].[Product]
SET [Name] = @0, [ProductSubcategoryID] = @1
WHERE (([ProductID] = @2) AND ([RowVersion] = @3))
SELECT [RowVersion]
FROM [Production].[Product]
WHERE @@ROWCOUNT > 0 AND [ProductID] = @2',N'@0 nvarchar(50),@1 int,@2 int,@3 binary(8)',@0=N'readerWriter2',@1=1,@2=999,@3=0x00000000000007D1

exec sp_executesql N'SELECT 
    [Limit1].[ProductID] AS [ProductID], 
    [Limit1].[C1] AS [C1], 
    [Limit1].[C2] AS [C2], 
    [Limit1].[C3] AS [C3], 
    [Limit1].[C4] AS [C4], 
    [Limit1].[C5] AS [C5]
    FROM ( SELECT TOP (2) 
        [Extent1].[ProductID] AS [ProductID], 
        CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X%'') THEN [Extent1].[ProductID] END AS [C1], 
        CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X%'') THEN [Extent1].[RowVersion] END AS [C2], 
        CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X%'') THEN [Extent1].[Name] END AS [C3], 
        CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X%'') THEN [Extent1].[ListPrice] END AS [C4], 
        CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X%'') THEN [Extent1].[ProductSubcategoryID] END AS [C5]
        FROM [Production].[Product] AS [Extent1]
        WHERE ((CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X%'') THEN [Extent1].[ProductID] END) = @p0) OR ((CASE WHEN (CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END LIKE ''0X%'') THEN [Extent1].[ProductID] END IS NULL) AND (@p0 IS NULL))
    )  AS [Limit1]',N'@p0 int',@p0=999

exec sp_executesql N'UPDATE [Production].[Product]
SET [ProductSubcategoryID] = @0
WHERE (([ProductID] = @1) AND ([RowVersion] = @2))
SELECT [RowVersion]
FROM [Production].[Product]
WHERE @@ROWCOUNT > 0 AND [ProductID] = @1',N'@0 int,@1 int,@2 binary(8)',@0=1,@1=999,@2=0x0000000000036338

exec sp_executesql N'SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[ProductID] AS [ProductID], 
    [Limit1].[RowVersion] AS [RowVersion], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM ( SELECT TOP (2) 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[RowVersion] AS [RowVersion], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ProductID] = @p0
    )  AS [Limit1]',N'@p0 int',@p0=999
