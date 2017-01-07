-- DefaultIfEmpty
SELECT [t].[ProductCategoryID], [t].[Name]
FROM (
    SELECT NULL AS [empty]
) AS [empty]
LEFT JOIN (
    SELECT [p].[ProductCategoryID], [p].[Name]
    FROM [Production].[ProductCategory] AS [p]
) AS [t] ON 1 = 1

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM   ( SELECT 1 AS X ) AS [SingleRowTable1]
    LEFT OUTER JOIN [Production].[ProductCategory] AS [Extent1] ON 1 = 1

-- DefaultIfEmptyWithPrimitive
SELECT [category].[ProductCategoryID]
FROM [Production].[ProductCategory] AS [category]

SELECT 
    CASE WHEN ([Project1].[C1] IS NULL) THEN -1 ELSE [Project1].[ProductCategoryID] END AS [C1]
    FROM   ( SELECT 1 AS X ) AS [SingleRowTable1]
    LEFT OUTER JOIN  (SELECT 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
        cast(1 as tinyint) AS [C1]
        FROM [Production].[ProductCategory] AS [Extent1] ) AS [Project1] ON 1 = 1

-- DefaultIfEmptyWithEntity
SELECT [p].[ProductCategoryID], [p].[Name]
FROM [Production].[ProductCategory] AS [p]
-- NotSupportedException.

-- Where
SELECT [category].[ProductCategoryID], [category].[Name]
FROM [Production].[ProductCategory] AS [category]
WHERE [category].[ProductCategoryID] > 0

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE [Extent1].[ProductCategoryID] > 0

-- WhereWithOr
SELECT [category].[ProductCategoryID], [category].[Name]
FROM [Production].[ProductCategory] AS [category]
WHERE ([category].[ProductCategoryID] <= 1) OR ([category].[ProductCategoryID] >= 4)

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE ([Extent1].[ProductCategoryID] <= 1) OR ([Extent1].[ProductCategoryID] >= 4)

-- WhereWithAnd
SELECT [category].[ProductCategoryID], [category].[Name]
FROM [Production].[ProductCategory] AS [category]
WHERE ([category].[ProductCategoryID] > 0) AND ([category].[ProductCategoryID] < 5)

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE ([Extent1].[ProductCategoryID] > 0) AND ([Extent1].[ProductCategoryID] < 5)

-- WhereAndWhere
SELECT [category].[ProductCategoryID], [category].[Name]
FROM [Production].[ProductCategory] AS [category]
WHERE ([category].[ProductCategoryID] > 0) AND ([category].[ProductCategoryID] < 5)

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE ([Extent1].[ProductCategoryID] > 0) AND ([Extent1].[ProductCategoryID] < 5)

-- WhereWithIs
SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID], [product].[RowVersion], [product].[Style]
FROM [Production].[Product] AS [product]
WHERE [product].[Style] IN (N'W', N'U', N'M') AND ([product].[Style] = N'U')

SELECT 
    '0X0X' AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[RowVersion] AS [RowVersion], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[Style] = N'U'

-- OfTypeWithEntiy
SELECT [p].[ProductID], [p].[ListPrice], [p].[Name], [p].[ProductSubcategoryID], [p].[RowVersion], [p].[Style]
FROM [Production].[Product] AS [p]
WHERE [p].[Style] = N'U'

SELECT 
    '0X0X' AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[RowVersion] AS [RowVersion], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[Style] = N'U'

-- OfTypeWithPromitive
SELECT [p].[ProductSubcategoryID]
FROM [Production].[Product] AS [p]
WHERE ([p].[Style] = N'W') OR (([p].[Style] = N'U') OR ([p].[Style] = N'M'))
-- NotSupportedException.

-- Select
SELECT [category].[Name] + [category].[Name]
FROM [Production].[ProductCategory] AS [category]

SELECT 
    [Extent1].[Name] + [Extent1].[Name] AS [C1]
    FROM [Production].[ProductCategory] AS [Extent1]

-- SelectWithStringConcat
SELECT [category].[Name] + [category].[Name]
FROM [Production].[ProductCategory] AS [category]

SELECT 
    [Extent1].[Name] + [Extent1].[Name] AS [C1]
    FROM [Production].[ProductCategory] AS [Extent1]

-- SelectAnonymousType
SELECT [product].[Name], CASE
    WHEN [product].[ListPrice] > 1000.0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Production].[Product] AS [product]
WHERE ([product].[Style] = N'W') OR (([product].[Style] = N'U') OR ([product].[Style] = N'M'))

SELECT 
    1 AS [C1], 
    [Extent1].[Name] AS [Name], 
    CASE 
        WHEN ([Extent1].[ListPrice] > cast(1000 as decimal(18))) THEN cast(1 as bit) 
        WHEN ( NOT ([Extent1].[ListPrice] > cast(1000 as decimal(18)))) THEN cast(0 as bit) 
    END AS [C2]
    FROM [Production].[Product] AS [Extent1]

-- GroupBy
SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [subcategory]
ORDER BY [subcategory].[ProductCategoryID]

SELECT 
    [Project2].[ProductCategoryID] AS [ProductCategoryID], 
    [Project2].[C1] AS [C1], 
    [Project2].[Name] AS [Name]
    FROM ( SELECT 
        [Distinct1].[ProductCategoryID] AS [ProductCategoryID], 
        [Extent2].[Name] AS [Name], 
        CASE WHEN ([Extent2].[ProductCategoryID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C1]
        FROM   (SELECT DISTINCT 
            [Extent1].[ProductCategoryID] AS [ProductCategoryID]
            FROM [Production].[ProductSubcategory] AS [Extent1] ) AS [Distinct1]
        LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Distinct1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
    )  AS [Project2]
    ORDER BY [Project2].[ProductCategoryID] ASC, [Project2].[C1] ASC

-- GroupByWithResultSelector
SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [subcategory]
ORDER BY [subcategory].[ProductCategoryID]

SELECT 
    [GroupBy1].[K1] AS [ProductCategoryID], 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        [Extent1].[ProductCategoryID] AS [K1], 
        COUNT(1) AS [A1]
        FROM [Production].[ProductSubcategory] AS [Extent1]
        GROUP BY [Extent1].[ProductCategoryID]
    )  AS [GroupBy1]

-- GroupByAndSelect
SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [subcategory]
ORDER BY [subcategory].[ProductCategoryID]

SELECT 
    [GroupBy1].[K1] AS [ProductCategoryID], 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        [Extent1].[ProductCategoryID] AS [K1], 
        COUNT(1) AS [A1]
        FROM [Production].[ProductSubcategory] AS [Extent1]
        GROUP BY [Extent1].[ProductCategoryID]
    )  AS [GroupBy1]

-- GroupByAndSelectMany
SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [subcategory]
ORDER BY [subcategory].[ProductCategoryID]

SELECT 
    [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[ProductCategoryID] AS [ProductCategoryID]
    FROM   (SELECT DISTINCT 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent1] ) AS [Distinct1]
    INNER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Distinct1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- GroupByMultipleKeys
SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID], [product].[RowVersion], [product].[Style]
FROM [Production].[Product] AS [product]
WHERE [product].[Style] IN (N'W', N'U', N'M')
ORDER BY [product].[ProductSubcategoryID], [product].[ListPrice]

SELECT 
    1 AS [C1], 
    [GroupBy1].[K2] AS [ProductSubcategoryID], 
    [GroupBy1].[K1] AS [ListPrice], 
    [GroupBy1].[A1] AS [C2]
    FROM ( SELECT 
        [Extent1].[ListPrice] AS [K1], 
        [Extent1].[ProductSubcategoryID] AS [K2], 
        COUNT(1) AS [A1]
        FROM [Production].[Product] AS [Extent1]
        GROUP BY [Extent1].[ListPrice], [Extent1].[ProductSubcategoryID]
    )  AS [GroupBy1]

-- InnerJoinWithJoin
SELECT [subcategory].[Name], [category].[Name]
FROM [Production].[ProductSubcategory] AS [subcategory]
INNER JOIN [Production].[ProductCategory] AS [category] ON [subcategory].[ProductCategoryID] = [category].[ProductCategoryID]

SELECT 
    [Extent2].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductSubcategory] AS [Extent1]
    INNER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- InnerJoinWithSelectMany
SELECT [subcategory].[Name], [category].[Name]
FROM [Production].[ProductSubcategory] AS [subcategory]
CROSS JOIN [Production].[ProductCategory] AS [category]
WHERE [subcategory].[ProductCategoryID] = [category].[ProductCategoryID]

SELECT 
    [Extent2].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductSubcategory] AS [Extent1]
    INNER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- InnerJoinWithGroupJoin
SELECT [subcategory].[Name], [category].[Name]
FROM [Production].[ProductSubcategory] AS [subcategory]
INNER JOIN [Production].[ProductCategory] AS [category] ON [subcategory].[ProductCategoryID] = [category].[ProductCategoryID]

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductSubcategory] AS [Extent1]
    INNER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- InnerJoinWithSelect
SELECT [subcategory].[Name], [category].[Name]
FROM [Production].[ProductSubcategory] AS [subcategory]
CROSS JOIN [Production].[ProductCategory] AS [category]
WHERE [category].[ProductCategoryID] = [subcategory].[ProductCategoryID]

SELECT 
    [Extent2].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductSubcategory] AS [Extent1]
    INNER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- InnerJoinWithAssociation
SELECT [subcategory].[Name], [subcategory.ProductCategory].[Name]
FROM [Production].[ProductSubcategory] AS [subcategory]
INNER JOIN [Production].[ProductCategory] AS [subcategory.ProductCategory] ON [subcategory].[ProductCategoryID] = [subcategory.ProductCategory].[ProductCategoryID]

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductSubcategory] AS [Extent1]
    INNER JOIN [Production].[ProductCategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- MultipleInnerJoinsWithAssociations
SELECT [product].[Name], [product.ProductProductPhotos.ProductPhoto].[LargePhotoFileName]
FROM [Production].[Product] AS [product]
INNER JOIN [Production].[ProductProductPhoto] AS [product.ProductProductPhotos] ON [product].[ProductID] = [product.ProductProductPhotos].[ProductID]
INNER JOIN [Production].[ProductPhoto] AS [product.ProductProductPhotos.ProductPhoto] ON [product.ProductProductPhotos].[ProductPhotoID] = [product.ProductProductPhotos.ProductPhoto].[ProductPhotoID]
WHERE ([product].[Style] = N'W') OR (([product].[Style] = N'U') OR ([product].[Style] = N'M'))

SELECT 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[Name] AS [Name], 
    [Extent3].[LargePhotoFileName] AS [LargePhotoFileName]
    FROM   [Production].[Product] AS [Extent1]
    INNER JOIN [Production].[ProductProductPhoto] AS [Extent2] ON [Extent1].[ProductID] = [Extent2].[ProductID]
    INNER JOIN [Production].[ProductPhoto] AS [Extent3] ON [Extent2].[ProductPhotoID] = [Extent3].[ProductPhotoID]

-- InnerJoinWithMultipleKeys
SELECT [subcategory].[Name], [category].[Name]
FROM [Production].[ProductSubcategory] AS [subcategory]
INNER JOIN [Production].[ProductCategory] AS [category] ON ([subcategory].[ProductCategoryID] = [category].[ProductCategoryID]) AND ([subcategory].[Name] = [category].[Name])

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductSubcategory] AS [Extent1]
    INNER JOIN [Production].[ProductCategory] AS [Extent2] ON ([Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]) AND ([Extent1].[Name] = [Extent2].[Name])

-- LeftOuterJoinWithGroupJoin
SELECT 
    [Project1].[ProductCategoryID] AS [ProductCategoryID], 
    [Project1].[Name] AS [Name], 
    [Project1].[C1] AS [C1], 
    [Project1].[Name1] AS [Name1]
    FROM ( SELECT 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
        [Extent1].[Name] AS [Name], 
        [Extent2].[Name] AS [Name1], 
        CASE WHEN ([Extent2].[ProductCategoryID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C1]
        FROM  [Production].[ProductCategory] AS [Extent1]
        LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
    )  AS [Project1]
    ORDER BY [Project1].[ProductCategoryID] ASC, [Project1].[C1] ASC

-- LeftOuterJoinWithSelect
SELECT 
    [Project1].[ProductCategoryID] AS [ProductCategoryID], 
    [Project1].[Name] AS [Name], 
    [Project1].[C1] AS [C1], 
    [Project1].[Name1] AS [Name1]
    FROM ( SELECT 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
        [Extent1].[Name] AS [Name], 
        [Extent2].[Name] AS [Name1], 
        CASE WHEN ([Extent2].[ProductCategoryID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C1]
        FROM  [Production].[ProductCategory] AS [Extent1]
        LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
    )  AS [Project1]
    ORDER BY [Project1].[ProductCategoryID] ASC, [Project1].[C1] ASC

-- LeftOuterJoinWithGroupJoinAndSelectMany
SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- LeftOuterJoinWithSelectAndSelectMany
SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent2].[ProductCategoryID] = [Extent1].[ProductCategoryID]

-- LeftOuterJoinWithAssociation
SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent2].[ProductCategoryID] = [Extent1].[ProductCategoryID]

-- CrossJoinWithSelectMany
SELECT 
    1 AS [C1], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[Product] AS [Extent1]
    CROSS JOIN [Production].[Product] AS [Extent2]
    WHERE ([Extent1].[ListPrice] > cast(2000 as decimal(18))) AND ([Extent2].[ListPrice] < cast(100 as decimal(18)))

-- CrossJoinWithJoin
SELECT 
    1 AS [C1], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[Product] AS [Extent1]
    INNER JOIN [Production].[Product] AS [Extent2] ON 1 = 1
    WHERE ([Extent1].[ListPrice] > cast(2000 as decimal(18))) AND ([Extent2].[ListPrice] < cast(100 as decimal(18)))

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
    )  AS [Project1]
    ORDER BY [Project1].[ProductID] ASC, [Project1].[C1] ASC

-- CROSS APPLY
SELECT [Left].[Count], [Right].[Value] FROM
    (SELECT [Count]
        FROM (VALUES (0), (1), (2), (3)) [0 to 4]([Count])) AS [Left]
    CROSS APPLY 
    (SELECT top ([Count]) [Value]
        FROM (VALUES (N'a'), (N'b'), (N'c'), (N'd')) [0 to 4]([Value])) AS [Right];

-- OUTER APPLY
SELECT [Left].[Count], [Right].[Value] FROM
    (SELECT [Count]
        FROM (VALUES (0), (1), (2), (3)) [0 to 4]([Count])) AS [Left]
    OUTER APPLY 
    (SELECT top ([Count]) [Value]
        FROM (VALUES (N'a'), (N'b'), (N'c'), (N'd')) [0 to 4]([Value])) AS [Right];

-- CrossApplyWithGroupByAndTake
SELECT 
    [Distinct1].[ProductCategoryID] AS [ProductCategoryID], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ProductCategoryID] AS [ProductCategoryID1]
    FROM   (SELECT DISTINCT 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent1] ) AS [Distinct1]
    CROSS APPLY  (SELECT TOP (1) 
        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent2].[Name] AS [Name], 
        [Extent2].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent2]
        WHERE [Distinct1].[ProductCategoryID] = [Extent2].[ProductCategoryID] ) AS [Limit1]

-- CrossApplyWithGroupJoinAndTake
SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Limit1].[Name] AS [Name1], 
    [Limit1].[ProductCategoryID] AS [ProductCategoryID1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    CROSS APPLY  (SELECT TOP (1) 
        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent2].[Name] AS [Name], 
        [Extent2].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent2]
        WHERE [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID] ) AS [Limit1]

-- CrossApplyWithAssociationAndTake
SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Limit1].[Name] AS [Name1], 
    [Limit1].[ProductCategoryID] AS [ProductCategoryID1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    CROSS APPLY  (SELECT TOP (1) 
        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent2].[Name] AS [Name], 
        [Extent2].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent2]
        WHERE [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID] ) AS [Limit1]

-- OuterApplyWithGroupByAndFirstOrDefault
SELECT 
    [Distinct1].[ProductCategoryID] AS [ProductCategoryID], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ProductCategoryID] AS [ProductCategoryID1]
    FROM   (SELECT DISTINCT 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent1] ) AS [Distinct1]
    OUTER APPLY  (SELECT TOP (1) 
        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent2].[Name] AS [Name], 
        [Extent2].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent2]
        WHERE [Distinct1].[ProductCategoryID] = [Extent2].[ProductCategoryID] ) AS [Limit1]

-- OuterApplyWithGroupJoinAndFirstOrDefault
SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Limit1].[Name] AS [Name1], 
    [Limit1].[ProductCategoryID] AS [ProductCategoryID1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    OUTER APPLY  (SELECT TOP (1) 
        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent2].[Name] AS [Name], 
        [Extent2].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent2]
        WHERE [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID] ) AS [Limit1]

-- OuterApplyWithAssociationAndFirstOrDefault
SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Limit1].[Name] AS [Name1], 
    [Limit1].[ProductCategoryID] AS [ProductCategoryID1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    OUTER APPLY  (SELECT TOP (1) 
        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent2].[Name] AS [Name], 
        [Extent2].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent2]
        WHERE [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID] ) AS [Limit1]

-- Concat
SELECT 
    [UnionAll1].[Name] AS [C1]
    FROM  (SELECT 
        [Extent1].[Name] AS [Name]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ListPrice] < cast(100 as decimal(18))
    UNION ALL
        SELECT 
        [Extent2].[Name] AS [Name]
        FROM [Production].[Product] AS [Extent2]
        WHERE [Extent2].[ListPrice] > cast(2000 as decimal(18))) AS [UnionAll1]

-- ConcatWithSelect
SELECT 
    [UnionAll1].[Name] AS [C1]
    FROM  (SELECT 
        [Extent1].[Name] AS [Name]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ListPrice] < cast(100 as decimal(18))
    UNION ALL
        SELECT 
        [Extent2].[Name] AS [Name]
        FROM [Production].[Product] AS [Extent2]
        WHERE [Extent2].[ListPrice] > cast(2000 as decimal(18))) AS [UnionAll1]

-- Distinct
SELECT 
    [Distinct1].[ProductCategoryID] AS [ProductCategoryID]
    FROM ( SELECT DISTINCT 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent1]
    )  AS [Distinct1]

-- DistinctWithGroupBy
SELECT 
    [Distinct1].[ProductCategoryID] AS [ProductCategoryID]
    FROM ( SELECT DISTINCT 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent1]
    )  AS [Distinct1]

-- DistinctMultipleKeys
SELECT 
    [Distinct1].[C1] AS [C1], 
    [Distinct1].[ProductCategoryID] AS [ProductCategoryID], 
    [Distinct1].[Name] AS [Name]
    FROM ( SELECT DISTINCT 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
        1 AS [C1]
        FROM [Production].[ProductSubcategory] AS [Extent1]
    )  AS [Distinct1]

-- DistinctMultipleKeysWithGroupBy
SELECT 
    [Distinct1].[C1] AS [C1], 
    [Distinct1].[ProductCategoryID] AS [ProductCategoryID], 
    [Distinct1].[Name] AS [Name]
    FROM ( SELECT DISTINCT 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
        1 AS [C1]
        FROM [Production].[ProductSubcategory] AS [Extent1]
    )  AS [Distinct1]

-- DistinctWithGroupByAndSelectAndFirstOrDefault
SELECT 
    (SELECT TOP (1) 
        [Extent2].[Name] AS [Name]
        FROM [Production].[ProductSubcategory] AS [Extent2]
        WHERE [Distinct1].[ProductCategoryID] = [Extent2].[ProductCategoryID]) AS [C1]
    FROM ( SELECT DISTINCT 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent1]
    )  AS [Distinct1]

-- Intersect
SELECT 
    [Intersect1].[C1] AS [C1], 
    [Intersect1].[Name] AS [C2], 
    [Intersect1].[ListPrice] AS [C3]
    FROM  (SELECT 
        1 AS [C1], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))
    INTERSECT
        SELECT 
        1 AS [C1], 
        [Extent2].[Name] AS [Name], 
        [Extent2].[ListPrice] AS [ListPrice]
        FROM [Production].[Product] AS [Extent2]
        WHERE [Extent2].[ListPrice] < cast(2000 as decimal(18))) AS [Intersect1]
    
-- Except
SELECT 
    [Except1].[C1] AS [C1], 
    [Except1].[Name] AS [C2], 
    [Except1].[ListPrice] AS [C3]
    FROM  (SELECT 
        1 AS [C1], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[ListPrice] AS [ListPrice]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))
    EXCEPT
        SELECT 
        1 AS [C1], 
        [Extent2].[Name] AS [Name], 
        [Extent2].[ListPrice] AS [ListPrice]
        FROM [Production].[Product] AS [Extent2]
        WHERE [Extent2].[ListPrice] > cast(2000 as decimal(18))) AS [Except1]

-- Skip
-- NotSupportedException.

-- OrderByAndSkip
SELECT 
    [Extent1].[Name] AS [Name]
    FROM [Production].[Product] AS [Extent1]
    ORDER BY [Extent1].[Name] ASC
    OFFSET 10 ROWS 

-- Take
SELECT TOP (10) 
    [c].[Name] AS [Name]
    FROM [Production].[Product] AS [c]

-- OrderByAndSkipAndTake
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

-- OrderByAndThenBy
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

-- OrderByAnonymousType
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

-- OrderByAndOrderBy
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

-- CastPrimitive
SELECT 
     CAST( [Extent1].[ListPrice] AS nvarchar(max)) AS [C1]
    FROM [Production].[Product] AS [Extent1]

-- CastEntity
-- NotSupportedException.

-- AsEnumerableAsQueryable
SELECT 
    1 AS [C1], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[ListPrice] > cast(0 as decimal(18))

SELECT 
    1 AS [C1], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice]
    FROM [Production].[Product] AS [Extent1]

-- SelectEntities
-- N/A

-- SelectEntityObjects
SELECT 
    [Extent1].[Name] AS [Name]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))

-- First
SELECT TOP (1) 
    [c].[Name] AS [Name]
    FROM [Production].[Product] AS [c]

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
        WHERE [Extent1].[ListPrice] > cast(5000 as decimal(18))
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
        WHERE [Extent1].[ListPrice] < cast(50 as decimal(18))
    )  AS [Limit1]

-- SingleOrDefault
SELECT 
    [Limit1].[C2] AS [C1], 
    [Limit1].[ListPrice] AS [ListPrice], 
    [Limit1].[C1] AS [C2]
    FROM ( SELECT TOP (2) 
        [GroupBy1].[A1] AS [C1], 
        [GroupBy1].[K1] AS [ListPrice], 
        1 AS [C2]
        FROM ( SELECT 
            [Extent1].[ListPrice] AS [K1], 
            COUNT(1) AS [A1]
            FROM [Production].[Product] AS [Extent1]
            GROUP BY [Extent1].[ListPrice]
        )  AS [GroupBy1]
        WHERE [GroupBy1].[A1] > 10
    )  AS [Limit1]

-- Count
SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        COUNT(1) AS [A1]
        FROM [Production].[ProductCategory] AS [Extent1]
    )  AS [GroupBy1]

-- LongCount
SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        COUNT_BIG(1) AS [A1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ListPrice] > cast(0 as decimal(18))
    )  AS [GroupBy1]

-- Max
SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        MAX([Extent1].[ModifiedDate]) AS [A1]
        FROM [Production].[ProductPhoto] AS [Extent1]
    )  AS [GroupBy1]

-- Min
SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        MIN([Extent1].[ListPrice]) AS [A1]
        FROM [Production].[Product] AS [Extent1]
    )  AS [GroupBy1]

-- Average
SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        AVG([Extent1].[ListPrice]) AS [A1]
        FROM [Production].[Product] AS [Extent1]
    )  AS [GroupBy1]

-- Sum
SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        SUM([Extent1].[ListPrice]) AS [A1]
        FROM [Production].[Product] AS [Extent1]
    )  AS [GroupBy1]

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
        WHERE cast(100 as decimal(18)) = [Extent1].[ListPrice]
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]

-- AnyWithPredicate
SELECT 
    CASE WHEN ( EXISTS (SELECT 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE cast(100 as decimal(18)) = [Extent1].[ListPrice]
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]

-- AllNot
SELECT 
    CASE WHEN ( NOT EXISTS (SELECT 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE ([Extent1].[ProductSubcategoryID] IS NULL) 
            OR (CASE -- OR and the succeeding condition is redundant.
                    WHEN ([Extent1].[ProductSubcategoryID] IS NOT NULL) THEN cast(1 as bit) 
                    ELSE cast(0 as bit) 
                END IS NULL)
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]

-- NotAny
SELECT 
    CASE WHEN ( EXISTS (SELECT 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ProductSubcategoryID] IS NULL
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
