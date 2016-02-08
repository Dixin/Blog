-- LazyLoading
SELECT 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductSubcategory] AS [Extent1]
-- EntityCommandExecutionException.

-- LazyLoadingWithToArray
SELECT 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductSubcategory] AS [Extent1]

exec sp_executesql N'SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE [Extent1].[ProductCategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=1

exec sp_executesql N'SELECT 
    CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[ProductSubcategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=1

-- ...

exec sp_executesql N'SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE [Extent1].[ProductCategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=4

-- ...

exec sp_executesql N'SELECT 
    CASE WHEN (((CASE WHEN ([Extent1].[Style] = N''M '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''U '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N''W '') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN ''0X'' WHEN ([Extent1].[Style] = N''M '') THEN ''0X0X'' WHEN ([Extent1].[Style] = N''U '') THEN ''0X1X'' ELSE ''0X2X'' END AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[ProductSubcategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=37

-- EagerLoadingWithSelect
SELECT 
    [Project1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Project1].[ProductCategoryID] AS [ProductCategoryID], 
    [Project1].[Name1] AS [Name], 
    [Project1].[Name] AS [Name1], 
    [Project1].[C1] AS [C1], 
    [Project1].[Name2] AS [Name2]
    FROM ( SELECT 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent1].[Name] AS [Name], 
        [Extent2].[ProductCategoryID] AS [ProductCategoryID], 
        [Extent2].[Name] AS [Name1], 
        [Extent3].[Name] AS [Name2], 
        CASE WHEN ([Extent3].[Name] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C1]
        FROM   [Production].[ProductSubcategory] AS [Extent1]
        LEFT OUTER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
        LEFT OUTER JOIN [Production].[Product] AS [Extent3] ON [Extent1].[ProductSubcategoryID] = [Extent3].[ProductSubcategoryID]
    )  AS [Project1]
    ORDER BY [Project1].[ProductSubcategoryID] ASC, [Project1].[ProductCategoryID] ASC, [Project1].[C1] ASC

-- EagerLoadingWithAssociation
SELECT 
    [Project1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Project1].[ProductCategoryID] AS [ProductCategoryID], 
    [Project1].[Name] AS [Name], 
    [Project1].[ProductCategoryID1] AS [ProductCategoryID1], 
    [Project1].[Name1] AS [Name1], 
    [Project1].[C2] AS [C1], 
    [Project1].[C1] AS [C2], 
    [Project1].[ProductID] AS [ProductID], 
    [Project1].[ProductSubcategoryID1] AS [ProductSubcategoryID1], 
    [Project1].[Name2] AS [Name2], 
    [Project1].[ListPrice] AS [ListPrice]
    FROM ( SELECT 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
        [Extent1].[Name] AS [Name], 
        [Extent2].[ProductCategoryID] AS [ProductCategoryID1], 
        [Extent2].[Name] AS [Name1], 
        [Extent3].[ProductID] AS [ProductID], 
        [Extent3].[ProductSubcategoryID] AS [ProductSubcategoryID1], 
        [Extent3].[Name] AS [Name2], 
        [Extent3].[ListPrice] AS [ListPrice], 
        CASE WHEN ([Extent3].[ProductID] IS NULL) THEN CAST(NULL AS varchar(1)) WHEN (((CASE WHEN ([Extent3].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent3].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent3].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '4X' WHEN ([Extent3].[Style] = N'M ') THEN '4X0X' WHEN ([Extent3].[Style] = N'U ') THEN '4X1X' ELSE '4X2X' END AS [C1], 
        CASE WHEN ([Extent3].[ProductID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C2]
        FROM   [Production].[ProductSubcategory] AS [Extent1]
        LEFT OUTER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
        LEFT OUTER JOIN [Production].[Product] AS [Extent3] ON [Extent1].[ProductSubcategoryID] = [Extent3].[ProductSubcategoryID]
    )  AS [Project1]
    ORDER BY [Project1].[ProductSubcategoryID] ASC, [Project1].[ProductCategoryID1] ASC, [Project1].[C2] ASC

-- ConditionalEagerLoading
-- ArgumentException.

-- ConditionalEagerLoadingWithSelect
SELECT 
    [Project1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Project1].[ProductCategoryID] AS [ProductCategoryID], 
    [Project1].[Name] AS [Name], 
    [Project1].[C2] AS [C1], 
    [Project1].[C1] AS [C2], 
    [Project1].[ProductID] AS [ProductID], 
    [Project1].[ProductSubcategoryID1] AS [ProductSubcategoryID1], 
    [Project1].[Name1] AS [Name1], 
    [Project1].[ListPrice] AS [ListPrice]
    FROM ( SELECT 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
        [Extent1].[Name] AS [Name], 
        [Extent2].[ProductID] AS [ProductID], 
        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID1], 
        [Extent2].[Name] AS [Name1], 
        [Extent2].[ListPrice] AS [ListPrice], 
        CASE WHEN ([Extent2].[ProductID] IS NULL) THEN CAST(NULL AS varchar(1)) WHEN (((CASE WHEN ([Extent2].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent2].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent2].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '2X' WHEN ([Extent2].[Style] = N'M ') THEN '2X0X' WHEN ([Extent2].[Style] = N'U ') THEN '2X1X' ELSE '2X2X' END AS [C1], 
        CASE WHEN ([Extent2].[ProductID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C2]
        FROM  [Production].[ProductSubcategory] AS [Extent1]
        LEFT OUTER JOIN [Production].[Product] AS [Extent2] ON ([Extent1].[ProductSubcategoryID] = [Extent2].[ProductSubcategoryID]) AND ([Extent2].[ListPrice] > cast(0 as decimal(18)))
    )  AS [Project1]
    ORDER BY [Project1].[ProductSubcategoryID] ASC, [Project1].[C2] ASC

-- NoLoading
SELECT 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductSubcategory] AS [Extent1]
