-- DefaultIfEmpty
SELECT 
    [Project1].[C1] AS [C1], 
    [Project1].[ProductID] AS [ProductID], 
    [Project1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Project1].[Name] AS [Name], 
    [Project1].[ListPrice] AS [ListPrice]
    FROM   ( SELECT 1 AS X ) AS [SingleRowTable1]
    LEFT OUTER JOIN  (SELECT 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1]
        FROM [Production].[Product] AS [Extent1] ) AS [Project1] ON 1 = 1

-- Where
SELECT 
    CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))

-- WhereWithOr
SELECT 
    CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice]
    FROM [Production].[Product] AS [Extent1]
    WHERE ([Extent1].[ListPrice] < cast(100 as decimal(18))) OR ([Extent1].[ListPrice] > cast(200 as decimal(18)))

-- WhereWithAnd
SELECT 
    CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice]
    FROM [Production].[Product] AS [Extent1]
    WHERE ([Extent1].[ListPrice] > cast(100 as decimal(18))) AND ([Extent1].[ListPrice] < cast(200 as decimal(18)))

-- WhereAndWhere
SELECT 
    CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice]
    FROM [Production].[Product] AS [Extent1]
    WHERE ([Extent1].[ListPrice] > cast(100 as decimal(18))) AND ([Extent1].[ListPrice] < cast(200 as decimal(18)))

-- WhereWithLike
SELECT 
    CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[Name] LIKE N'ML %'

-- WhereWithLikeMethod
-- NotSupportedException.

-- WhereWithContains
SELECT 
    CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[Name] IN (N'Blade', N'Chainring', N'Freewheel')

-- WhereWithNull
SELECT 
    CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[ProductSubcategoryID] IS NOT NULL

-- OfType
SELECT 
    '0X0X' AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[Style] = N'U '

-- Select
SELECT 
     CAST( [Extent1].[ProductID] AS nvarchar(max)) + N': ' + [Extent1].[Name] AS [C1]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))

-- SelectWithStringConcat
SELECT 
     CAST( [Extent1].[ProductID] AS nvarchar(max)) + N': ' + [Extent1].[Name] AS [C1]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))

-- SelectAnonymousType
SELECT 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))

-- SelectEntity
-- N/A

-- SelectEntityObjects
SELECT 
    [Extent1].[Name] AS [Name]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))

-- SelectWithCase
SELECT 
    1 AS [C1], 
    [Extent1].[Name] AS [Name], 
    CASE WHEN ([Extent1].[ListPrice] > cast(0 as decimal(18))) THEN cast(1 as bit) WHEN ( NOT ([Extent1].[ListPrice] > cast(0 as decimal(18)))) THEN cast(0 as bit) END AS [C2]
    FROM [Production].[Product] AS [Extent1]

-- Grouping
SELECT 
    [Project2].[C2] AS [C1], 
    [Project2].[C1] AS [C2], 
    [Project2].[C3] AS [C3], 
    [Project2].[Name] AS [Name]
    FROM ( SELECT 
        [Distinct1].[C1] AS [C1], 
        1 AS [C2], 
        [Extent2].[Name] AS [Name], 
        CASE WHEN ([Extent2].[Name] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C3]
        FROM   (SELECT DISTINCT 
            SUBSTRING([Extent1].[Name], 0 + 1, 1) AS [C1]
            FROM [Production].[Product] AS [Extent1] ) AS [Distinct1]
        LEFT OUTER JOIN [Production].[Product] AS [Extent2] ON ([Distinct1].[C1] = (SUBSTRING([Extent2].[Name], 0 + 1, 1))) OR (([Distinct1].[C1] IS NULL) AND (SUBSTRING([Extent2].[Name], 0 + 1, 1) IS NULL))
    )  AS [Project2]
    ORDER BY [Project2].[C1] ASC, [Project2].[C3] ASC

-- GroupBy
SELECT 
    1 AS [C1], 
    [GroupBy1].[K1] AS [C2], 
    [GroupBy1].[A1] AS [C3]
    FROM ( SELECT 
        [Extent1].[K1] AS [K1], 
        COUNT([Extent1].[A1]) AS [A1]
        FROM ( SELECT 
            SUBSTRING([Extent1].[Name], 0 + 1, 1) AS [K1], 
            1 AS [A1]
            FROM [Production].[Product] AS [Extent1]
        )  AS [Extent1]
        GROUP BY [K1]
    )  AS [GroupBy1]

-- GroupByWithWhere
SELECT 
    1 AS [C1], 
    [GroupBy1].[K1] AS [C2], 
    [GroupBy1].[A1] AS [C3]
    FROM ( SELECT 
        [Extent1].[K1] AS [K1], 
        COUNT([Extent1].[A1]) AS [A1]
        FROM ( SELECT 
            SUBSTRING([Extent1].[Name], 0 + 1, 1) AS [K1], 
            1 AS [A1]
            FROM [Production].[Product] AS [Extent1]
        )  AS [Extent1]
        GROUP BY [K1]
    )  AS [GroupBy1]
    WHERE [GroupBy1].[A1] > 0

-- InnerJoin
SELECT 
    [Extent2].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductSubcategory] AS [Extent1]
    INNER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- InnerJoinWithSelectMany
SELECT 
    [Extent2].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductSubcategory] AS [Extent1]
    INNER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- InnerJoinWithAssociation
SELECT 
    [Extent2].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductSubcategory] AS [Extent1]
    INNER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- InnerJoinWithMultipleKeys
SELECT 
    [Extent2].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductSubcategory] AS [Extent1]
    INNER JOIN [Production].[ProductCategory] AS [Extent2] ON ((CASE WHEN ([Extent1].[ProductCategoryID] IS NULL) THEN -1 ELSE [Extent1].[ProductCategoryID] END) = [Extent2].[ProductCategoryID]) AND (((SUBSTRING([Extent1].[Name], 0 + 1, 1)) = (SUBSTRING([Extent2].[Name], 0 + 1, 1))) OR ((SUBSTRING([Extent1].[Name], 0 + 1, 1) IS NULL) AND (SUBSTRING([Extent2].[Name], 0 + 1, 1) IS NULL)))

-- LeftOuterJoin
SELECT 
    [Project1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Project1].[Name] AS [Name], 
    [Project1].[C1] AS [C1], 
    [Project1].[Name1] AS [Name1]
    FROM ( SELECT 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent1].[Name] AS [Name], 
        [Extent2].[Name] AS [Name1], 
        CASE WHEN ([Extent2].[ProductCategoryID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C1]
        FROM  [Production].[ProductSubcategory] AS [Extent1]
        LEFT OUTER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
    )  AS [Project1]
    ORDER BY [Project1].[ProductSubcategoryID] ASC, [Project1].[C1] ASC

-- LeftOuterJoinWithDefaultIfEmpty
SELECT 
    1 AS [C1], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductSubcategory] AS [Extent1]
    LEFT OUTER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- LeftOuterJoinWithSelect
SELECT 
    [Project1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Project1].[Name] AS [Name], 
    [Project1].[C1] AS [C1], 
    [Project1].[Name1] AS [Name1]
    FROM ( SELECT 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent1].[Name] AS [Name], 
        [Extent2].[Name] AS [Name1], 
        CASE WHEN ([Extent2].[ProductCategoryID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C1]
        FROM  [Production].[ProductSubcategory] AS [Extent1]
        LEFT OUTER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
    )  AS [Project1]
    ORDER BY [Project1].[ProductSubcategoryID] ASC, [Project1].[C1] ASC

-- LeftOuterJoinWithAssociation
SELECT 
    1 AS [C1], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductSubcategory] AS [Extent1]
    LEFT OUTER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- CrossJoin
SELECT 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[Name] AS [Name], 
    [Extent3].[LargePhotoFileName] AS [LargePhotoFileName]
    FROM   [Production].[Product] AS [Extent1]
    INNER JOIN [Production].[ProductProductPhoto] AS [Extent2] ON [Extent1].[ProductID] = [Extent2].[ProductID]
    INNER JOIN [Production].[ProductPhoto] AS [Extent3] ON [Extent2].[ProductPhotoID] = [Extent3].[ProductPhotoID]

-- CrossJoinWithSelectMany
SELECT 
    1 AS [C1], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductSubcategory] AS [Extent1]
    CROSS JOIN [Production].[ProductCategory] AS [Extent2]

-- CrossJoinWithJoin
SELECT 
    1 AS [C1], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductSubcategory] AS [Extent1]
    INNER JOIN [Production].[ProductCategory] AS [Extent2] ON 1 = 1

-- SelfJoin
SELECT 
    [Project1].[ProductID] AS [ProductID], 
    [Project1].[Name] AS [Name], 
    [Project1].[ListPrice] AS [ListPrice], 
    [Project1].[C1] AS [C1], 
    [Project1].[Name1] AS [Name1]
    FROM ( SELECT 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        [Extent2].[Name] AS [Name1], 
        CASE WHEN ([Extent2].[ProductID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C1]
        FROM  [Production].[Product] AS [Extent1]
        LEFT OUTER JOIN [Production].[Product] AS [Extent2] ON ([Extent1].[ListPrice] = [Extent2].[ListPrice]) AND ([Extent2].[ProductID] <> [Extent1].[ProductID])
        WHERE [Extent1].[ListPrice] > cast(0 as decimal(18))
    )  AS [Project1]
    ORDER BY [Project1].[ProductID] ASC, [Project1].[C1] ASC

-- Concat
SELECT 
    [UnionAll1].[C1] AS [C1], 
    [UnionAll1].[Name] AS [C2], 
    [UnionAll1].[ListPrice] AS [C3]
    FROM  (SELECT 
        1 AS [C1], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ListPrice] < cast(100 as decimal(18))
    UNION ALL
        SELECT 
        1 AS [C1], 
        [Extent2].[Name] AS [Name], 
        [Extent2].[ListPrice] AS [ListPrice]
        FROM [Production].[Product] AS [Extent2]
        WHERE [Extent2].[ListPrice] > cast(200 as decimal(18))) AS [UnionAll1]

-- ConcatWithSelect
SELECT 
    1 AS [C1], 
    [UnionAll1].[Name] AS [C2], 
    [UnionAll1].[ListPrice] AS [C3]
    FROM  (SELECT 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ListPrice] < cast(100 as decimal(18))
    UNION ALL
        SELECT 
        [Extent2].[Name] AS [Name], 
        [Extent2].[ListPrice] AS [ListPrice]
        FROM [Production].[Product] AS [Extent2]
        WHERE [Extent2].[ListPrice] > cast(200 as decimal(18))) AS [UnionAll1]

-- Distinct
SELECT 
    [Distinct1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM ( SELECT DISTINCT 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID]
        FROM [Production].[Product] AS [Extent1]
    )  AS [Distinct1]

-- DistinctWithGroupByAndSelect
SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Limit1].[Name] AS [Name]
    FROM   (SELECT DISTINCT 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID]
        FROM [Production].[Product] AS [Extent1] ) AS [Distinct1]
    OUTER APPLY  (SELECT TOP (1) 
        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent2].[Name] AS [Name], 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent2]
        WHERE ([Distinct1].[ProductSubcategoryID] = [Extent2].[ProductSubcategoryID]) OR (([Distinct1].[ProductSubcategoryID] IS NULL) AND ([Extent2].[ProductSubcategoryID] IS NULL)) ) AS [Limit1]

-- DistinctWithGroupByAndSelectMany
SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Limit1].[Name] AS [Name]
    FROM   (SELECT DISTINCT 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID]
        FROM [Production].[Product] AS [Extent1] ) AS [Distinct1]
    CROSS APPLY  (SELECT TOP (1) 
        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent2].[Name] AS [Name], 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent2]
        WHERE ([Distinct1].[ProductSubcategoryID] = [Extent2].[ProductSubcategoryID]) OR (([Distinct1].[ProductSubcategoryID] IS NULL) AND ([Extent2].[ProductSubcategoryID] IS NULL)) ) AS [Limit1]

-- Intersect
SELECT 
    [Intersect1].[ProductID] AS [C1], 
    [Intersect1].[Name] AS [C2], 
    [Intersect1].[ListPrice] AS [C3]
    FROM  (SELECT 
        CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))
    INTERSECT
        SELECT 
        CASE WHEN (((CASE WHEN ([Extent2].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent2].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent2].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent2].[Style] = N'M ') THEN '0X0X' WHEN ([Extent2].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
        [Extent2].[ProductID] AS [ProductID], 
        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent2].[Name] AS [Name], 
        [Extent2].[ListPrice] AS [ListPrice]
        FROM [Production].[Product] AS [Extent2]
        WHERE [Extent2].[ListPrice] < cast(200 as decimal(18))) AS [Intersect1]
	
-- Except
SELECT 
    [Except1].[ProductID] AS [C1], 
    [Except1].[Name] AS [C2], 
    [Except1].[ListPrice] AS [C3]
    FROM  (SELECT 
        CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))
    EXCEPT
        SELECT 
        CASE WHEN (((CASE WHEN ([Extent2].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent2].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent2].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent2].[Style] = N'M ') THEN '0X0X' WHEN ([Extent2].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
        [Extent2].[ProductID] AS [ProductID], 
        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent2].[Name] AS [Name], 
        [Extent2].[ListPrice] AS [ListPrice]
        FROM [Production].[Product] AS [Extent2]
        WHERE [Extent2].[ListPrice] > cast(200 as decimal(18))) AS [Except1]

-- Skip
-- NotSupportedException.

-- OrderBySkip
SELECT 
    [Extent1].[Name] AS [Name]
    FROM [Production].[Product] AS [Extent1]
    ORDER BY [Extent1].[Name] ASC
    OFFSET 10 ROWS 

-- Take
SELECT TOP (10) 
    [c].[Name] AS [Name]
    FROM [Production].[Product] AS [c]

-- SkipTake
SELECT 
    [Extent1].[Name] AS [Name]
    FROM [Production].[Product] AS [Extent1]
    ORDER BY [Extent1].[Name] ASC
    OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY 

-- OrderBy
SELECT 
    [Project1].[C1] AS [C1], 
    [Project1].[Name] AS [Name], 
    [Project1].[ListPrice] AS [ListPrice]
    FROM ( SELECT 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
    )  AS [Project1]
    ORDER BY [Project1].[ListPrice] ASC

-- OrderByDescending
SELECT 
    [Project1].[C1] AS [C1], 
    [Project1].[Name] AS [Name], 
    [Project1].[ListPrice] AS [ListPrice]
    FROM ( SELECT 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
    )  AS [Project1]
    ORDER BY [Project1].[ListPrice] DESC

-- OrderByThenBy
SELECT 
    [Project1].[C1] AS [C1], 
    [Project1].[Name] AS [Name], 
    [Project1].[ListPrice] AS [ListPrice]
    FROM ( SELECT 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
    )  AS [Project1]
    ORDER BY [Project1].[ListPrice] ASC, [Project1].[Name] ASC

-- OrderByOrderBy
SELECT 
    [Project1].[C1] AS [C1], 
    [Project1].[Name] AS [Name], 
    [Project1].[ListPrice] AS [ListPrice]
    FROM ( SELECT 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
    )  AS [Project1]
    ORDER BY [Project1].[Name] ASC

-- Reverse
-- NotSupportedException.

-- Cast
-- NotSupportedException.

-- First
SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice]
    FROM ( SELECT TOP (1) 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
    )  AS [Limit1]

-- FirstOrDefault
SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice]
    FROM ( SELECT TOP (1) 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE cast(1 as decimal(18)) = [Extent1].[ListPrice]
    )  AS [Limit1]

-- Single
SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice]
    FROM ( SELECT TOP (2) 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE 539.99 = [Extent1].[ListPrice]
    )  AS [Limit1]

-- SingleOrDefault
SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice]
    FROM ( SELECT TOP (2) 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice], 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE cast(540 as decimal(18)) = [Extent1].[ListPrice]
    )  AS [Limit1]

-- Count
SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        COUNT(1) AS [A1]
        FROM [Production].[Product] AS [Extent1]
        WHERE cast(0 as decimal(18)) = [Extent1].[ListPrice]
    )  AS [GroupBy1]

-- LongCount
SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        COUNT_BIG(1) AS [A1]
        FROM [Production].[Product] AS [Extent1]
    )  AS [GroupBy1]

-- Min
SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        MIN([Extent1].[ListPrice]) AS [A1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ListPrice] > cast(0 as decimal(18))
    )  AS [GroupBy1]

-- Max
SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        MAX([Extent1].[ListPrice]) AS [A1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[Style] = N'U '
    )  AS [GroupBy1]

-- Sum
SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        SUM([Extent1].[ListPrice]) AS [A1]
        FROM [Production].[Product] AS [Extent1]
    )  AS [GroupBy1]

-- Average
SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        AVG([Extent1].[ListPrice]) AS [A1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ListPrice] > cast(0 as decimal(18))
    )  AS [GroupBy1]

-- All
SELECT 
    CASE WHEN ( NOT EXISTS (SELECT 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE ( NOT ([Extent1].[ListPrice] > cast(0 as decimal(18)))) OR (CASE WHEN ([Extent1].[ListPrice] > cast(0 as decimal(18))) THEN cast(1 as bit) WHEN ( NOT ([Extent1].[ListPrice] > cast(0 as decimal(18)))) THEN cast(0 as bit) END IS NULL)
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]

-- Any
SELECT 
    CASE WHEN ( EXISTS (SELECT 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]

-- Contains
SELECT 
    CASE WHEN ( EXISTS (SELECT 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE 9.99 = [Extent1].[ListPrice]
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
	
-- InlinePredicate
SELECT 
    [Extent1].[Name] AS [Name]
    FROM [Production].[Product] AS [Extent1]
    WHERE ([Extent1].[ListPrice] > cast(0 as decimal(18))) AND ([Extent1].[ProductSubcategoryID] IS NOT NULL)

-- InlinePredicateCompiled
SELECT 
    [Extent1].[Name] AS [Name]
    FROM [Production].[Product] AS [Extent1]
    WHERE ([Extent1].[ListPrice] > cast(0 as decimal(18))) AND ([Extent1].[ProductSubcategoryID] IS NOT NULL)

-- MethodPredicate
-- NotSupportedException

-- MethodPredicateCompiled
-- NotSupportedException

-- MethodSelector
-- NotSupportedException

-- LocalSelector
SELECT 
    CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[ProductID] > 100

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
