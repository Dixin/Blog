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
SELECT [transaction].[TransactionID], [transaction].[ActualCost], [transaction].[ProductID], [transaction].[Quantity], [transaction].[TransactionDate], [transaction].[TransactionType]
FROM [Production].[TransactionHistory] AS [transaction]
WHERE [transaction].[TransactionType] IN (N'W', N'S', N'P') AND ([transaction].[TransactionType] = N'S')

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
SELECT [t].[TransactionID], [t].[ActualCost], [t].[ProductID], [t].[Quantity], [t].[TransactionDate], [t].[TransactionType]
FROM [Production].[TransactionHistory] AS [t]
WHERE [t].[TransactionType] = N'W'

SELECT 
    '0X0X' AS [C1], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[RowVersion] AS [RowVersion], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ListPrice] AS [ListPrice], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[Style] = N'U'

-- OfTypeWithPrimitive
SELECT [p].[ProductSubcategoryID]
FROM [Production].[Product] AS [p]

SELECT [p].[ProductSubcategoryID]
FROM [Production].[Product] AS [p]
WHERE ([p].[Style] = N'W') OR (([p].[Style] = N'U') OR ([p].[Style] = N'M'))
-- NotSupportedException.

-- Select
SELECT ([person].[FirstName] + N' ') + [person].[LastName]
FROM [Person].[Person] AS [person]

SELECT 
    [Extent1].[Name] + [Extent1].[Name] AS [C1]
    FROM [Production].[ProductCategory] AS [Extent1]

-- SelectWithStringConcat
SELECT [category].[FirstName], [category].[LastName]
FROM [Person].[Person] AS [category]

SELECT 
    [Extent1].[Name] + [Extent1].[Name] AS [C1]
    FROM [Production].[ProductCategory] AS [Extent1]

-- SelectAnonymousType
SELECT [product].[Name], CASE
    WHEN [product].[ListPrice] > 1000.0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Production].[Product] AS [product]

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
SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID], [product].[RowVersion]
FROM [Production].[Product] AS [product]
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
SELECT [category].[Name], [subcategory].[Name]
FROM [Production].[ProductCategory] AS [category]
INNER JOIN [Production].[ProductSubcategory] AS [subcategory] ON [category].[ProductCategoryID] = [subcategory].[ProductCategoryID]

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    INNER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- InnerJoinWithSelect
SELECT [category].[Name], [subcategory].[Name]
FROM [Production].[ProductCategory] AS [category]
CROSS JOIN [Production].[ProductSubcategory] AS [subcategory]
WHERE [category].[ProductCategoryID] = [subcategory].[ProductCategoryID]

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    INNER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- InnerJoinWithSelectMany
SELECT [category].[Name], [subcategory].[Name]
FROM [Production].[ProductCategory] AS [category]
CROSS JOIN [Production].[ProductSubcategory] AS [subcategory]
WHERE [category].[ProductCategoryID] = [subcategory].[ProductCategoryID]

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    INNER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- InnerJoinWithSelectAndRelationship
SELECT [category].[Name], [category.ProductSubcategories].[Name]
FROM [Production].[ProductCategory] AS [category]
INNER JOIN [Production].[ProductSubcategory] AS [category.ProductSubcategories] ON [category].[ProductCategoryID] = [category.ProductSubcategories].[ProductCategoryID]

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    INNER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- InnerJoinWithSelectManyAndRelationship
SELECT [category].[Name], [category.ProductSubcategories].[Name]
FROM [Production].[ProductCategory] AS [category]
INNER JOIN [Production].[ProductSubcategory] AS [category.ProductSubcategories] ON [category].[ProductCategoryID] = [category.ProductSubcategories].[ProductCategoryID]

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    INNER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- InnerJoinWithMultipleKeys
SELECT [product].[Name], [transaction].[Quantity]
FROM [Production].[Product] AS [product]
INNER JOIN [Production].[TransactionHistory] AS [transaction] ON ([product].[ProductID] = [transaction].[ProductID]) AND ([product].[ListPrice] = ([transaction].[ActualCost] / [transaction].[Quantity]))

SELECT 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Quantity] AS [Quantity]
    FROM  [Production].[Product] AS [Extent1]
    INNER JOIN [Production].[TransactionHistory] AS [Extent2] ON ([Extent1].[ProductID] = [Extent2].[ProductID]) AND ([Extent1].[ListPrice] = ([Extent2].[ActualCost] /  CAST( [Extent2].[Quantity] AS decimal(19,0))))
    WHERE [Extent2].[TransactionType] IN (N'P',N'S',N'W')

-- MultipleInnerJoinsWithRelationship
SELECT [product].[Name], [product.ProductProductPhotos.ProductPhoto].[LargePhotoFileName]
FROM [Production].[Product] AS [product]
INNER JOIN [Production].[ProductProductPhoto] AS [product.ProductProductPhotos] ON [product].[ProductID] = [product.ProductProductPhotos].[ProductID]
INNER JOIN [Production].[ProductPhoto] AS [product.ProductProductPhotos.ProductPhoto] ON [product.ProductProductPhotos].[ProductPhotoID] = [product.ProductProductPhotos.ProductPhoto].[ProductPhotoID]

SELECT 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[Name] AS [Name], 
    [Extent3].[LargePhotoFileName] AS [LargePhotoFileName]
    FROM   [Production].[Product] AS [Extent1]
    INNER JOIN [Production].[ProductProductPhoto] AS [Extent2] ON [Extent1].[ProductID] = [Extent2].[ProductID]
    INNER JOIN [Production].[ProductPhoto] AS [Extent3] ON [Extent2].[ProductPhotoID] = [Extent3].[ProductPhotoID]

-- InnerJoinWithGroupJoinAndSelectMany
SELECT [category].[Name], [subcategory].[Name]
FROM [Production].[ProductCategory] AS [category]
INNER JOIN [Production].[ProductSubcategory] AS [subcategory] ON [category].[ProductCategoryID] = [subcategory].[ProductCategoryID]

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    INNER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- LeftOuterJoinWithGroupJoin
SELECT [category].[ProductCategoryID], [category].[Name], [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
FROM [Production].[ProductCategory] AS [category]
LEFT JOIN [Production].[ProductSubcategory] AS [subcategory] ON [category].[ProductCategoryID] = [subcategory].[ProductCategoryID]
ORDER BY [category].[ProductCategoryID]

SELECT 
    [Project1].[ProductCategoryID] AS [ProductCategoryID], 
    [Project1].[Name] AS [Name], 
    [Project1].[C1] AS [C1], 
    [Project1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Project1].[Name1] AS [Name1], 
    [Project1].[ProductCategoryID1] AS [ProductCategoryID1]
    FROM ( SELECT 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
        [Extent1].[Name] AS [Name], 
        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent2].[Name] AS [Name1], 
        [Extent2].[ProductCategoryID] AS [ProductCategoryID1], 
        CASE WHEN ([Extent2].[ProductSubcategoryID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C1]
        FROM  [Production].[ProductCategory] AS [Extent1]
        LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
    )  AS [Project1]
    ORDER BY [Project1].[ProductCategoryID] ASC, [Project1].[C1] ASC

-- LeftOuterJoinWithGroupJoinAndSelectMany
SELECT [category].[ProductCategoryID], [category].[Name], [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
FROM [Production].[ProductCategory] AS [category]
LEFT JOIN [Production].[ProductSubcategory] AS [subcategory] ON [category].[ProductCategoryID] = [subcategory].[ProductCategoryID]
ORDER BY [category].[ProductCategoryID]

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent2].[Name] AS [Name1], 
    [Extent2].[ProductCategoryID] AS [ProductCategoryID1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- LeftOuterJoinWithSelect
SELECT [category].[Name], [t1].[Name]
FROM [Production].[ProductCategory] AS [category]
CROSS APPLY (
    SELECT [t0].*
    FROM (
        SELECT NULL AS [empty]
    ) AS [empty0]
    LEFT JOIN (
        SELECT [subcategory0].*
        FROM [Production].[ProductSubcategory] AS [subcategory0]
        WHERE [category].[ProductCategoryID] = [subcategory0].[ProductCategoryID]
    ) AS [t0] ON 1 = 1
) AS [t1]

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- LeftOuterJoinWithSelectMany
SELECT [category].[Name], [t1].[Name]
FROM [Production].[ProductCategory] AS [category]
CROSS APPLY (
    SELECT [t0].*
    FROM (
        SELECT NULL AS [empty]
    ) AS [empty0]
    LEFT JOIN (
        SELECT [subcategory0].*
        FROM [Production].[ProductSubcategory] AS [subcategory0]
        WHERE [category].[ProductCategoryID] = [subcategory0].[ProductCategoryID]
    ) AS [t0] ON 1 = 1
) AS [t1]

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- LeftOuterJoinWithSelectAndRelationship
-- ArgumentOutOfRangeException

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- LeftOuterJoinWithSelectManyAndRelationship
-- ArgumentOutOfRangeException

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[ProductCategory] AS [Extent1]
    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]

-- CrossJoinWithSelectMany
SELECT [product].[Name], [product0].[Name]
FROM [Production].[Product] AS [product]
CROSS JOIN [Production].[Product] AS [product0]
WHERE ([product].[ListPrice] > 2000.0) AND ([product0].[ListPrice] < 100.0)

SELECT 
    1 AS [C1], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[Product] AS [Extent1]
    CROSS JOIN [Production].[Product] AS [Extent2]
    WHERE ([Extent1].[ListPrice] > cast(2000 as decimal(18))) AND ([Extent2].[ListPrice] < cast(100 as decimal(18)))

-- CrossJoinWithJoin
SELECT [product].[Name], [t].[Name]
FROM [Production].[Product] AS [product]
INNER JOIN (
    SELECT [product1].*
    FROM [Production].[Product] AS [product1]
    WHERE [product1].[ListPrice] < 100.0
) AS [t] ON 1 = 1
WHERE [product].[ListPrice] > 2000.0

SELECT 
    1 AS [C1], 
    [Extent1].[Name] AS [Name], 
    [Extent2].[Name] AS [Name1]
    FROM  [Production].[Product] AS [Extent1]
    INNER JOIN [Production].[Product] AS [Extent2] ON 1 = 1
    WHERE ([Extent1].[ListPrice] > cast(2000 as decimal(18))) AND ([Extent2].[ListPrice] < cast(100 as decimal(18)))

-- SelfJoin
SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID], [product].[RowVersion], [product0].[ProductID], [product0].[ListPrice], [product0].[Name], [product0].[ProductSubcategoryID], [product0].[RowVersion]
FROM [Production].[Product] AS [product]
LEFT JOIN [Production].[Product] AS [product0] ON [product].[ListPrice] = [product0].[ListPrice]
ORDER BY [product].[ListPrice]

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
        FROM (VALUES (N'a'), (N'b'), (N'c'), (N'd')) [a to d]([Value])) AS [Right];

-- OUTER APPLY
SELECT [Left].[Count], [Right].[Value] FROM
    (SELECT [Count]
        FROM (VALUES (0), (1), (2), (3)) [0 to 4]([Count])) AS [Left]
    OUTER APPLY 
    (SELECT top ([Count]) [Value]
        FROM (VALUES (N'a'), (N'b'), (N'c'), (N'd')) [a to d]([Value])) AS [Right];

-- CrossApplyWithGroupByAndTake
SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [subcategory]
ORDER BY [subcategory].[ProductCategoryID]

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
SELECT [category].[ProductCategoryID], [category].[Name], [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
FROM [Production].[ProductCategory] AS [category]
LEFT JOIN [Production].[ProductSubcategory] AS [subcategory] ON [category].[ProductCategoryID] = [subcategory].[ProductCategoryID]
ORDER BY [category].[ProductCategoryID]

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
SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [subcategory]
ORDER BY [subcategory].[ProductCategoryID]

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
SELECT [category].[ProductCategoryID], [category].[Name], [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
FROM [Production].[ProductCategory] AS [category]
LEFT JOIN [Production].[ProductSubcategory] AS [subcategory] ON [category].[ProductCategoryID] = [subcategory].[ProductCategoryID]
ORDER BY [category].[ProductCategoryID]

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
SELECT [category].[ProductCategoryID], [category].[Name]
FROM [Production].[ProductCategory] AS [category]

exec sp_executesql N'SELECT TOP(1) [p].[ProductSubcategoryID], [p].[Name], [p].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [p]
WHERE @_outer_ProductCategoryID = [p].[ProductCategoryID]',N'@_outer_ProductCategoryID int',@_outer_ProductCategoryID=4

exec sp_executesql N'SELECT TOP(1) [p].[ProductSubcategoryID], [p].[Name], [p].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [p]
WHERE @_outer_ProductCategoryID = [p].[ProductCategoryID]',N'@_outer_ProductCategoryID int',@_outer_ProductCategoryID=1

exec sp_executesql N'SELECT TOP(1) [p].[ProductSubcategoryID], [p].[Name], [p].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [p]
WHERE @_outer_ProductCategoryID = [p].[ProductCategoryID]',N'@_outer_ProductCategoryID int',@_outer_ProductCategoryID=3

exec sp_executesql N'SELECT TOP(1) [p].[ProductSubcategoryID], [p].[Name], [p].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [p]
WHERE @_outer_ProductCategoryID = [p].[ProductCategoryID]',N'@_outer_ProductCategoryID int',@_outer_ProductCategoryID=2

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
SELECT [product].[Name]
FROM [Production].[Product] AS [product]
WHERE [product].[ListPrice] < 100.0

SELECT [product0].[Name]
FROM [Production].[Product] AS [product0]
WHERE [product0].[ListPrice] > 2000.0

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
SELECT DISTINCT [subcategory].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [subcategory]

SELECT 
    [Distinct1].[ProductCategoryID] AS [ProductCategoryID]
    FROM ( SELECT DISTINCT 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent1]
    )  AS [Distinct1]

-- DistinctWithGroupBy
SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [subcategory]
ORDER BY [subcategory].[ProductCategoryID]

SELECT 
    [Distinct1].[ProductCategoryID] AS [ProductCategoryID]
    FROM ( SELECT DISTINCT 
        [Extent1].[ProductCategoryID] AS [ProductCategoryID]
        FROM [Production].[ProductSubcategory] AS [Extent1]
    )  AS [Distinct1]

-- DistinctMultipleKeys
SELECT DISTINCT [subcategory].[ProductCategoryID], [subcategory].[Name]
FROM [Production].[ProductSubcategory] AS [subcategory]

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
SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [subcategory]
ORDER BY [subcategory].[ProductCategoryID], [subcategory].[Name]

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
SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [subcategory]
ORDER BY [subcategory].[ProductCategoryID]

SELECT 
    [Limit1].[ProductID] AS [ProductID], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[ListPrice] AS [ListPrice], 
    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Limit1].[RowVersion] AS [RowVersion]
    FROM   (SELECT DISTINCT 
        [Extent1].[ListPrice] AS [ListPrice]
        FROM [Production].[Product] AS [Extent1] ) AS [Distinct1]
    OUTER APPLY  (SELECT TOP (1) 
        [Extent2].[ProductID] AS [ProductID], 
        [Extent2].[Name] AS [Name], 
        [Extent2].[ListPrice] AS [ListPrice], 
        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
        [Extent2].[RowVersion] AS [RowVersion]
        FROM [Production].[Product] AS [Extent2]
        WHERE [Distinct1].[ListPrice] = [Extent2].[ListPrice] ) AS [Limit1]

-- Intersect
SELECT [product0].[Name], [product0].[ListPrice]
FROM [Production].[Product] AS [product0]
WHERE [product0].[ListPrice] < 2000.0

SELECT [product].[Name], [product].[ListPrice]
FROM [Production].[Product] AS [product]
WHERE [product].[ListPrice] > 100.0

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
SELECT [product0].[Name], [product0].[ListPrice]
FROM [Production].[Product] AS [product0]
WHERE [product0].[ListPrice] > 2000.0

SELECT [product].[Name], [product].[ListPrice]
FROM [Production].[Product] AS [product]
WHERE [product].[ListPrice] > 100.0

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
exec sp_executesql N'SELECT [t].[Name]
FROM (
    SELECT [p0].*
    FROM [Production].[Product] AS [p0]
    ORDER BY (SELECT 1)
    OFFSET @__p_0 ROWS
) AS [t]',N'@__p_0 int',@__p_0=10

-- NotSupportedException.

-- OrderByAndSkip
exec sp_executesql N'SELECT [product].[Name]
FROM [Production].[Product] AS [product]
ORDER BY [product].[Name]
OFFSET @__p_0 ROWS',N'@__p_0 int',@__p_0=10

SELECT 
    [Extent1].[Name] AS [Name]
    FROM [Production].[Product] AS [Extent1]
    ORDER BY [Extent1].[Name] ASC
    OFFSET 10 ROWS 

-- Take
exec sp_executesql N'SELECT [t].[Name]
FROM (
    SELECT TOP(@__p_0) [p0].*
    FROM [Production].[Product] AS [p0]
) AS [t]',N'@__p_0 int',@__p_0=10

SELECT TOP (10) 
    [c].[Name] AS [Name]
    FROM [Production].[Product] AS [c]

-- OrderByAndSkipAndTake
exec sp_executesql N'SELECT [t].[Name]
FROM (
    SELECT [product0].*
    FROM [Production].[Product] AS [product0]
    ORDER BY [product0].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]',N'@__p_0 int,@__p_1 int',@__p_0=20,@__p_1=10

SELECT 
    [Extent1].[Name] AS [Name]
    FROM [Production].[Product] AS [Extent1]
    ORDER BY [Extent1].[Name] ASC
    OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY

-- OrderBy
SELECT [product].[Name], [product].[ListPrice]
FROM [Production].[Product] AS [product]
ORDER BY [product].[ListPrice]

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
SELECT [product].[Name], [product].[ListPrice]
FROM [Production].[Product] AS [product]
ORDER BY [product].[ListPrice] DESC

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
SELECT [product].[Name], [product].[ListPrice]
FROM [Production].[Product] AS [product]
ORDER BY [product].[ListPrice], [product].[Name]

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
SELECT [product].[Name], [product].[ListPrice]
FROM [Production].[Product] AS [product]
ORDER BY (SELECT 1)

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
SELECT [product].[Name], [product].[ListPrice]
FROM [Production].[Product] AS [product]
ORDER BY [product].[Name], [product].[ListPrice]

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
SELECT [product].[TransactionID], [product].[ActualCost], [product].[ProductID], [product].[Quantity], [product].[TransactionDate], [product].[TransactionType]
FROM [Production].[TransactionHistory] AS [product]
WHERE [product].[TransactionType] IN (N'W', N'S', N'P') AND ([product].[ActualCost] > 500.0)

-- NotSupportedException.

-- AsEnumerableAsQueryable
SELECT [product].[Name], [product].[ListPrice]
FROM [Production].[Product] AS [product]
WHERE [product].[ListPrice] > 0.0

SELECT [product].[Name], [product].[ListPrice]
FROM [Production].[Product] AS [product]

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
SELECT [product].[ProductID], [product].[Name]
FROM [Production].[Product] AS [product]
WHERE [product].[ListPrice] > 1000.0

-- NotSupportedException

-- SelectEntityObjects
SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID], [product].[RowVersion]
FROM [Production].[Product] AS [product]
WHERE [product].[ListPrice] > 1000.0

SELECT 
    [Extent1].[Name] AS [Name]
    FROM [Production].[Product] AS [Extent1]
    WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))

-- First
SELECT TOP(1) [product].[Name]
FROM [Production].[Product] AS [product]

SELECT TOP (1) 
    [c].[Name] AS [Name]
    FROM [Production].[Product] AS [c]

-- FirstOrDefault
SELECT TOP(1) [product].[Name], [product].[ListPrice]
FROM [Production].[Product] AS [product]
WHERE [product].[ListPrice] > 5000.0

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

-- Last
SELECT [p].[ProductID], [p].[ListPrice], [p].[Name], [p].[ProductSubcategoryID], [p].[RowVersion]
FROM [Production].[Product] AS [p]

-- NotSupportedException

-- LastOrDefault
SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID], [product].[RowVersion]
FROM [Production].[Product] AS [product]
WHERE [product].[ListPrice] < 0.0

-- NotSupportedException

-- Single
SELECT TOP(2) [product].[Name], [product].[ListPrice]
FROM [Production].[Product] AS [product]
WHERE [product].[ListPrice] < 50.0

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
SELECT [subcategory].[ProductID], [subcategory].[ListPrice], [subcategory].[Name], [subcategory].[ProductSubcategoryID], [subcategory].[RowVersion]
FROM [Production].[Product] AS [subcategory]
ORDER BY [subcategory].[ListPrice]

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
SELECT COUNT(*)
FROM [Production].[ProductCategory] AS [p]

SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        COUNT(1) AS [A1]
        FROM [Production].[ProductCategory] AS [Extent1]
    )  AS [GroupBy1]

-- LongCount
SELECT COUNT_BIG(*)
FROM [Production].[Product] AS [product]
WHERE [product].[ListPrice] > 0.0

SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        COUNT_BIG(1) AS [A1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ListPrice] > cast(0 as decimal(18))
    )  AS [GroupBy1]

-- Max
SELECT MAX([photo].[ModifiedDate])
FROM [Production].[ProductPhoto] AS [photo]

SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        MAX([Extent1].[ModifiedDate]) AS [A1]
        FROM [Production].[ProductPhoto] AS [Extent1]
    )  AS [GroupBy1]

-- Min
SELECT MIN([product].[ListPrice])
FROM [Production].[Product] AS [product]

SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        MIN([Extent1].[ListPrice]) AS [A1]
        FROM [Production].[Product] AS [Extent1]
    )  AS [GroupBy1]

-- Average
SELECT [product].[ListPrice]
FROM [Production].[Product] AS [product]

SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        AVG([Extent1].[ListPrice]) AS [A1]
        FROM [Production].[Product] AS [Extent1]
    )  AS [GroupBy1]

-- AverageWithSelector
SELECT [product].[ListPrice]
FROM [Production].[Product] AS [product]

-- Sum
SELECT SUM([product].[ListPrice])
FROM [Production].[Product] AS [product]

SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        SUM([Extent1].[ListPrice]) AS [A1]
        FROM [Production].[Product] AS [Extent1]
    )  AS [GroupBy1]

-- Any
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Production].[Product] AS [p])
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT 
    CASE WHEN ( EXISTS (SELECT 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]

-- Contains
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Production].[Product] AS [product]
        WHERE [product].[ListPrice] = 100.0)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT 
    CASE WHEN ( EXISTS (SELECT 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE cast(100 as decimal(18)) = [Extent1].[ListPrice]
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]

-- Any
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Production].[Product] AS [p])
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT 
    CASE WHEN ( EXISTS (SELECT 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]

-- AnyWithPredicate
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Production].[Product] AS [product]
        WHERE [product].[ListPrice] > 10.0)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT 
    CASE WHEN ( EXISTS (SELECT 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ListPrice] > cast(10 as decimal(18))
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]

-- AllWithPredicate
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Production].[Product] AS [product]
        WHERE [product].[ListPrice] <= 10.0)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT 
    CASE WHEN ( NOT EXISTS (SELECT 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE ( NOT ([Extent1].[ListPrice] > cast(10 as decimal(18)))) OR (
			CASE 
				WHEN ([Extent1].[ListPrice] > cast(10 as decimal(18))) THEN cast(1 as bit) 
				WHEN ( NOT ([Extent1].[ListPrice] > cast(10 as decimal(18)))) THEN cast(0 as bit) 
			END IS NULL)
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]

SELECT 
    CASE WHEN ( EXISTS (SELECT 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE cast(100 as decimal(18)) = [Extent1].[ListPrice]
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]

-- AllNot
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Production].[Product] AS [product]
        WHERE [product].[ProductSubcategoryID] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

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
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Production].[Product] AS [product]
        WHERE [product].[ProductSubcategoryID] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT 
    CASE WHEN ( EXISTS (SELECT 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ProductSubcategoryID] IS NULL
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
