-- ImplicitLazyLoading
SELECT TOP (1) 
    [c].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [c].[Name] AS [Name], 
    [c].[ProductCategoryID] AS [ProductCategoryID]
    FROM [Production].[ProductSubcategory] AS [c]

exec sp_executesql N'SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE [Extent1].[ProductCategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=1

exec sp_executesql N'SELECT 
    CASE 
        WHEN (
            ((CASE 
                WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) 
                ELSE cast(0 as bit) 
            END) <> 1) AND 
            ((CASE 
                WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit)
                ELSE cast(0 as bit)
            END) <> 1) AND 
            ((CASE
                WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) 
                ELSE cast(0 as bit) 
            END) <> 1)) THEN ''0X''
        WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X''
        WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X''
        ELSE ''0X2X'' 
    END AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[RowVersion] AS [RowVersion], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[ProductSubcategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=1

-- ExplicitLazyLoading
SELECT TOP (1) 
    [c].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [c].[Name] AS [Name], 
    [c].[ProductCategoryID] AS [ProductCategoryID]
    FROM [Production].[ProductSubcategory] AS [c]

exec sp_executesql N'SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE [Extent1].[ProductCategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=1

exec sp_executesql N'SELECT 
    CASE 
        WHEN (
            ((CASE 
                WHEN ([Extent1].[Style] = N''M'') THEN cast(1 as bit) 
                ELSE cast(0 as bit) 
            END) <> 1) AND 
            ((CASE 
                WHEN ([Extent1].[Style] = N''U'') THEN cast(1 as bit)
                ELSE cast(0 as bit)
            END) <> 1) AND 
            ((CASE
                WHEN ([Extent1].[Style] = N''W'') THEN cast(1 as bit) 
                ELSE cast(0 as bit) 
            END) <> 1)) THEN ''0X''
        WHEN ([Extent1].[Style] = N''M'') THEN ''0X0X''
        WHEN ([Extent1].[Style] = N''U'') THEN ''0X1X''
        ELSE ''0X2X''
    END AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[RowVersion] AS [RowVersion], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[ProductSubcategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=1

-- ExplicitLazyLoadingWithQuery
exec sp_executesql N'SELECT 
    [Limit1].[Name] AS [Name]
    FROM ( SELECT TOP (2) 
        [Extent1].[Name] AS [Name]
        FROM [Production].[ProductCategory] AS [Extent1]
        WHERE [Extent1].[ProductCategoryID] = @EntityKeyValue1
    )  AS [Limit1]',N'@EntityKeyValue1 int',@EntityKeyValue1=1

exec sp_executesql N'SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        COUNT(1) AS [A1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ProductSubcategoryID] = @EntityKeyValue1
    )  AS [GroupBy1]',N'@EntityKeyValue1 int',@EntityKeyValue1=1

-- LazyLoadingAndDeferredExecution
SELECT 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductSubcategory] AS [Extent1]
-- EntityCommandExecutionException.

-- LazyLoadingAndImmediateExecution
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

-- EagerLoadingWithInclude
SELECT 
    [Project1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Project1].[Name] AS [Name], 
    [Project1].[ProductCategoryID] AS [ProductCategoryID], 
    [Project1].[ProductCategoryID1] AS [ProductCategoryID1], 
    [Project1].[Name1] AS [Name1], 
    [Project1].[C2] AS [C1], 
    [Project1].[C1] AS [C2], 
    [Project1].[ProductID] AS [ProductID], 
    [Project1].[RowVersion] AS [RowVersion], 
    [Project1].[Name2] AS [Name2], 
    [Project1].[ListPrice] AS [ListPrice], 
    [Project1].[ProductSubcategoryID1] AS [ProductSubcategoryID1]
    FROM ( SELECT 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
        [Extent2].[ProductCategoryID] AS [ProductCategoryID1], 
        [Extent2].[Name] AS [Name1], 
        [Extent3].[ProductID] AS [ProductID], 
        [Extent3].[RowVersion] AS [RowVersion], 
        [Extent3].[Name] AS [Name2], 
        [Extent3].[ListPrice] AS [ListPrice], 
        [Extent3].[ProductSubcategoryID] AS [ProductSubcategoryID1], 
        CASE 
            WHEN ([Extent3].[ProductID] IS NULL) THEN CAST(NULL AS varchar(1)) 
            WHEN (
                ((CASE 
                    WHEN ([Extent3].[Style] = N'M') THEN cast(1 as bit) 
                    ELSE cast(0 as bit) 
                END) <> 1) AND 
                ((CASE 
                    WHEN ([Extent3].[Style] = N'U') THEN cast(1 as bit) 
                    ELSE cast(0 as bit) 
                END) <> 1) AND 
                ((CASE 
                    WHEN ([Extent3].[Style] = N'W') THEN cast(1 as bit) 
                    ELSE cast(0 as bit) 
                END) <> 1)) THEN '4X' 
            WHEN ([Extent3].[Style] = N'M') THEN '4X0X' 
            WHEN ([Extent3].[Style] = N'U') THEN '4X1X' 
            ELSE '4X2X' 
        END AS [C1], 
        CASE 
            WHEN ([Extent3].[ProductID] IS NULL) THEN CAST(NULL AS int) 
            ELSE 1 
        END AS [C2]
        FROM   [Production].[ProductSubcategory] AS [Extent1]
        INNER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
        LEFT OUTER JOIN [Production].[Product] AS [Extent3] ON [Extent1].[ProductSubcategoryID] = [Extent3].[ProductSubcategoryID]
    )  AS [Project1]
    ORDER BY [Project1].[ProductSubcategoryID] ASC, [Project1].[ProductCategoryID1] ASC, [Project1].[C2] ASC

-- EagerLoadingWithIncludeAndSelect
SELECT 
    [Project1].[ProductCategoryID] AS [ProductCategoryID], 
    [Project1].[Name] AS [Name], 
    [Project1].[C3] AS [C1], 
    [Project1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Project1].[Name1] AS [Name1], 
    [Project1].[ProductCategoryID1] AS [ProductCategoryID1], 
    [Project1].[C2] AS [C2], 
    [Project1].[C1] AS [C3], 
    [Project1].[ProductID] AS [ProductID], 
    [Project1].[RowVersion] AS [RowVersion], 
    [Project1].[Name2] AS [Name2], 
    [Project1].[ListPrice] AS [ListPrice], 
    [Project1].[ProductSubcategoryID1] AS [ProductSubcategoryID1]
    FROM ( SELECT 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
        [Extent1].[Name] AS [Name], 
        [Join1].[ProductSubcategoryID1] AS [ProductSubcategoryID], 
        [Join1].[Name1] AS [Name1], 
        [Join1].[ProductCategoryID] AS [ProductCategoryID1], 
        [Join1].[ProductID] AS [ProductID], 
        [Join1].[RowVersion] AS [RowVersion], 
        [Join1].[Name2] AS [Name2], 
        [Join1].[ListPrice] AS [ListPrice], 
        [Join1].[ProductSubcategoryID2] AS [ProductSubcategoryID1], 
        CASE
            WHEN ([Join1].[ProductSubcategoryID1] IS NULL) THEN CAST(NULL AS varchar(1))
            WHEN ([Join1].[ProductID] IS NULL) THEN CAST(NULL AS varchar(1))
            WHEN (
                ((CASE
                    WHEN ([Join1].[Style] = N'M') THEN CAST(1 AS bit)
                    ELSE CAST(0 AS bit)
                END) <> 1) AND
                ((CASE
                    WHEN ([Join1].[Style] = N'U') THEN CAST(1 AS bit)
                    ELSE CAST(0 AS bit)
                END) <> 1) AND
                ((CASE
                    WHEN ([Join1].[Style] = N'W') THEN CAST(1 AS bit)
                    ELSE CAST(0 AS bit)
                END) <> 1)) THEN '4X'
            WHEN ([Join1].[Style] = N'M') THEN '4X0X'
            WHEN ([Join1].[Style] = N'U') THEN '4X1X'
            ELSE '4X2X'
        END AS [C1],
        CASE
            WHEN ([Join1].[ProductSubcategoryID1] IS NULL) THEN CAST(NULL AS int)
            WHEN ([Join1].[ProductID] IS NULL) THEN CAST(NULL AS int)
            ELSE 1
        END AS [C2],
        CASE
            WHEN ([Join1].[ProductSubcategoryID1] IS NULL) THEN CAST(NULL AS int)
            ELSE 1
        END AS [C3]
        FROM  [Production].[ProductCategory] AS [Extent1]
        LEFT OUTER JOIN  (SELECT 
            [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID1], 
            [Extent2].[Name] AS [Name1], 
            [Extent2].[ProductCategoryID] AS [ProductCategoryID], 
            [Extent3].[ProductID] AS [ProductID], 
            [Extent3].[RowVersion] AS [RowVersion], 
            [Extent3].[Name] AS [Name2], 
            [Extent3].[ListPrice] AS [ListPrice], 
            [Extent3].[ProductSubcategoryID] AS [ProductSubcategoryID2], 
            [Extent3].[Style] AS [Style]
            FROM  [Production].[ProductSubcategory] AS [Extent2]
            LEFT OUTER JOIN [Production].[Product] AS [Extent3] 
            ON [Extent2].[ProductSubcategoryID] = [Extent3].[ProductSubcategoryID] ) AS [Join1] 
        ON [Extent1].[ProductCategoryID] = [Join1].[ProductCategoryID]
    )  AS [Project1]
    ORDER BY [Project1].[ProductCategoryID] ASC, [Project1].[C3] ASC, [Project1].[ProductSubcategoryID] ASC, [Project1].[C2] ASC

-- EagerLoadingWithSelect
SELECT 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1], 
    (SELECT 
        COUNT(1) AS [A1]
        FROM [Production].[Product] AS [Extent3]
        WHERE [Extent1].[ProductSubcategoryID] = [Extent3].[ProductSubcategoryID]) AS [C1]
    FROM  [Production].[ProductSubcategory] AS [Extent1]
    INNER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- PrintSubcategoriesWithLazyLoading
SELECT 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ProductCategoryID] AS [ProductCategoryID]
    FROM   (SELECT DISTINCT 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent1] ) AS [Distinct1]
    OUTER APPLY  (SELECT TOP (1) 
        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent2].[Name] AS [Name], 
        [Extent2].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent2]
        WHERE [Distinct1].[ProductCategoryID] = [Extent2].[ProductCategoryID] ) AS [Limit1]

exec sp_executesql N'SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE [Extent1].[ProductCategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=1

exec sp_executesql N'SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE [Extent1].[ProductCategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=2

exec sp_executesql N'SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE [Extent1].[ProductCategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=3

exec sp_executesql N'SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE [Extent1].[ProductCategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=4

-- PrintSubcategoriesWithEagerLoading
SELECT 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Limit1].[Name1] AS [Name], 
    [Limit1].[ProductCategoryID1] AS [ProductCategoryID], 
    [Limit1].[ProductCategoryID] AS [ProductCategoryID1], 
    [Limit1].[Name] AS [Name1]
    FROM   (SELECT DISTINCT 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent1] ) AS [Distinct1]
    OUTER APPLY  (SELECT TOP (1) 
        [Extent3].[ProductCategoryID] AS [ProductCategoryID], 
        [Extent3].[Name] AS [Name], 
        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent2].[Name] AS [Name1], 
        [Extent2].[ProductCategoryID] AS [ProductCategoryID1]
        FROM  [Production].[ProductSubcategory] AS [Extent2]
        INNER JOIN [Production].[ProductCategory] AS [Extent3] ON [Extent2].[ProductCategoryID] = [Extent3].[ProductCategoryID]
        WHERE [Distinct1].[ProductCategoryID] = [Extent2].[ProductCategoryID] ) AS [Limit1]

-- ConditionalEagerLoadingWithInclude
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

-- DisableLazyLoading
SELECT 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductSubcategory] AS [Extent1]
