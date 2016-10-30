-- DefaultIfEmpty
SELECT [t2].[test], [t2].[ProductCategoryID], [t2].[Name]
FROM (
    SELECT NULL AS [EMPTY]
    ) AS [t0]
LEFT OUTER JOIN (
    SELECT 1 AS [test], [t1].[ProductCategoryID], [t1].[Name]
    FROM [Production].[ProductCategory] AS [t1]
    ) AS [t2] ON 1=1 

-- DefaultIfEmptyWithPrimitive
-- NotSupportedException.

-- DefaultIfEmptyWithEntity
-- NotSupportedException.

-- Where
exec sp_executesql N'SELECT [t0].[ProductCategoryID], [t0].[Name]
FROM [Production].[ProductCategory] AS [t0]
WHERE [t0].[ProductCategoryID] > @p0',N'@p0 int',@p0=0

exec sp_executesql N'SELECT [t0].[ProductCategoryID], [t0].[Name]
FROM [Production].[ProductCategory] AS [t0]
WHERE ([t0].[ProductCategoryID] <= @p0) OR ([t0].[ProductCategoryID] >= @p1)',N'@p0 int,@p1 int',@p0=1,@p1=4

-- WhereWithAnd
exec sp_executesql N'SELECT [t0].[ProductCategoryID], [t0].[Name]
FROM [Production].[ProductCategory] AS [t0]
WHERE ([t0].[ProductCategoryID] > @p0) AND ([t0].[ProductCategoryID] < @p1)',N'@p0 int,@p1 int',@p0=0,@p1=5

-- WhereAndWhere
exec sp_executesql N'SELECT [t0].[ProductCategoryID], [t0].[Name]
FROM [Production].[ProductCategory] AS [t0]
WHERE ([t0].[ProductCategoryID] < @p0) AND ([t0].[ProductCategoryID] > @p1)',N'@p0 int,@p1 int',@p0=5,@p1=0

-- WhereWithIs
exec sp_executesql N'SELECT [t0].[Style], [t0].[RowVersion], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE (([t0].[Style] <> @p0) AND ([t0].[Style] <> @p1)) OR ([t0].[Style] IS NULL)',N'@p0 nchar(2),@p1 nchar(2)',@p0=N'W ',@p1=N'M '

-- OfTypeWithEntiy
exec sp_executesql N'SELECT [t0].[Style], [t0].[RowVersion], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE (([t0].[Style] <> @p0) AND ([t0].[Style] <> @p1)) OR ([t0].[Style] IS NULL)',N'@p0 nchar(2),@p1 nchar(2)',@p0=N'W ',@p1=N'M '

-- OfTypeWithPromitive
SELECT [t0].[ProductID]
FROM [Production].[Product] AS [t0]

-- Select
SELECT [t0].[Name] + [t0].[Name] AS [value]
FROM [Production].[ProductCategory] AS [t0]

-- SelectWithStringConcat
SELECT [t0].[Name] + [t0].[Name] AS [value]
FROM [Production].[ProductCategory] AS [t0]

-- SelectAnonymousType
exec sp_executesql N'SELECT [t0].[Name], 
    (CASE 
        WHEN [t0].[ListPrice] > @p0 THEN 1
        WHEN NOT ([t0].[ListPrice] > @p0) THEN 0
        ELSE NULL
     END) AS [IsExpensive]
FROM [Production].[Product] AS [t0]',N'@p0 decimal(33,4)',@p0=1000.0000

-- GroupBy
SELECT COUNT(*) AS [SubcategoryCount], [t0].[ProductCategoryID] AS [CategoryID]
FROM [Production].[ProductSubcategory] AS [t0]
GROUP BY [t0].[ProductCategoryID]

exec sp_executesql N'SELECT [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE @x1 = [t0].[ProductCategoryID]',N'@x1 int',@x1=1

exec sp_executesql N'SELECT [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE @x1 = [t0].[ProductCategoryID]',N'@x1 int',@x1=2

exec sp_executesql N'SELECT [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE @x1 = [t0].[ProductCategoryID]',N'@x1 int',@x1=3

exec sp_executesql N'SELECT [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE @x1 = [t0].[ProductCategoryID]',N'@x1 int',@x1=4

-- GroupByWithResultSelector
SELECT COUNT(*) AS [SubcategoryCount], [t0].[ProductCategoryID] AS [CategoryID]
FROM [Production].[ProductSubcategory] AS [t0]
GROUP BY [t0].[ProductCategoryID]

-- GroupByAndSelect
SELECT COUNT(*) AS [SubcategoryCount], [t0].[ProductCategoryID] AS [CategoryID]
FROM [Production].[ProductSubcategory] AS [t0]
GROUP BY [t0].[ProductCategoryID]

-- GroupByAndSelectMany
SELECT [t2].[ProductCategoryID], [t2].[ProductSubcategoryID], [t2].[Name]
FROM (
    SELECT [t0].[ProductCategoryID]
    FROM [Production].[ProductSubcategory] AS [t0]
    GROUP BY [t0].[ProductCategoryID]
    ) AS [t1]
CROSS JOIN [Production].[ProductSubcategory] AS [t2]
WHERE [t1].[ProductCategoryID] = [t2].[ProductCategoryID]

-- GroupByMultipleKeys
SELECT COUNT(*) AS [Count], [t0].[ProductSubcategoryID], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
GROUP BY [t0].[ProductSubcategoryID], [t0].[ListPrice]

-- InnerJoinWithJoin
SELECT [t0].[Name] AS [Subcategory], [t1].[Name] AS [Category]
FROM [Production].[ProductSubcategory] AS [t0]
INNER JOIN [Production].[ProductCategory] AS [t1] ON [t0].[ProductCategoryID] = [t1].[ProductCategoryID]

-- InnerJoinWithSelectMany
SELECT [t0].[Name] AS [Subcategory], [t1].[Name] AS [Category]
FROM [Production].[ProductSubcategory] AS [t0], [Production].[ProductCategory] AS [t1]
WHERE [t0].[ProductCategoryID] = [t1].[ProductCategoryID]

-- InnerJoinWithGroupJoin
SELECT [t0].[Name] AS [Subcategory], [t1].[Name] AS [Category]
FROM [Production].[ProductSubcategory] AS [t0], [Production].[ProductCategory] AS [t1]
WHERE [t0].[ProductCategoryID] = [t1].[ProductCategoryID]

-- InnerJoinWithSelect
SELECT [t0].[Name] AS [Subcategory], [t1].[Name] AS [Category]
FROM [Production].[ProductSubcategory] AS [t0], [Production].[ProductCategory] AS [t1]
WHERE [t1].[ProductCategoryID] = [t0].[ProductCategoryID]

-- InnerJoinWithAssociation
SELECT [t0].[Name] AS [Subcategory], [t1].[Name] AS [Category]
FROM [Production].[ProductSubcategory] AS [t0]
INNER JOIN [Production].[ProductCategory] AS [t1] ON [t1].[ProductCategoryID] = [t0].[ProductCategoryID]

-- MultipleInnerJoinsWithAssociations
SELECT [t0].[Name] AS [Product], [t2].[LargePhotoFileName] AS [Photo]
FROM [Production].[Product] AS [t0]
CROSS JOIN [Production].[ProductProductPhoto] AS [t1]
LEFT OUTER JOIN [Production].[ProductPhoto] AS [t2] ON [t2].[ProductPhotoID] = [t1].[ProductPhotoID]
WHERE [t1].[ProductID] = [t0].[ProductID]

-- InnerJoinWithMultipleKeys
SELECT [t0].[Name] AS [Subcategory], [t1].[Name] AS [Category]
FROM [Production].[ProductSubcategory] AS [t0]
INNER JOIN [Production].[ProductCategory] AS [t1] ON ([t0].[ProductCategoryID] = [t1].[ProductCategoryID]) AND ([t0].[Name] = [t1].[Name])

-- LeftOuterJoinWithGroupJoin
SELECT [t0].[Name] AS [Subcategory], [t1].[Name] AS [Category]
FROM [Production].[ProductSubcategory] AS [t0], [Production].[ProductCategory] AS [t1]
WHERE [t0].[ProductCategoryID] = [t1].[ProductCategoryID]

-- LeftOuterJoinWithSelect
SELECT [t0].[Name] AS [Category], [t1].[Name], (
    SELECT COUNT(*)
    FROM [Production].[ProductSubcategory] AS [t2]
    WHERE [t0].[ProductCategoryID] = [t2].[ProductCategoryID]
    ) AS [value]
FROM [Production].[ProductCategory] AS [t0]
LEFT OUTER JOIN [Production].[ProductSubcategory] AS [t1] ON [t0].[ProductCategoryID] = [t1].[ProductCategoryID]
ORDER BY [t0].[ProductCategoryID], [t1].[ProductSubcategoryID]

-- LeftOuterJoinWithGroupJoinAndSelectMany
SELECT [t0].[Name] AS [Category], [t1].[Name], (
    SELECT COUNT(*)
    FROM [Production].[ProductSubcategory] AS [t2]
    WHERE [t2].[ProductCategoryID] = [t0].[ProductCategoryID]
    ) AS [value]
FROM [Production].[ProductCategory] AS [t0]
LEFT OUTER JOIN [Production].[ProductSubcategory] AS [t1] ON [t1].[ProductCategoryID] = [t0].[ProductCategoryID]
ORDER BY [t0].[ProductCategoryID], [t1].[ProductSubcategoryID]

-- LeftOuterJoinWithSelectAndSelectMany
SELECT [t0].[Name] AS [Category], [t1].[Name] AS [Subcategory]
FROM [Production].[ProductCategory] AS [t0]
LEFT OUTER JOIN [Production].[ProductSubcategory] AS [t1] ON [t0].[ProductCategoryID] = [t1].[ProductCategoryID]

-- LeftOuterJoinWithAssociation
SELECT [t0].[Name] AS [Category], [t1].[Name] AS [Subcategory]
FROM [Production].[ProductCategory] AS [t0]
LEFT OUTER JOIN [Production].[ProductSubcategory] AS [t1] ON [t1].[ProductCategoryID] = [t0].[ProductCategoryID]

-- CrossJoinWithSelectMany
exec sp_executesql N'SELECT [t0].[Name] AS [Expensive], [t1].[Name] AS [Cheap]
FROM [Production].[Product] AS [t0], [Production].[Product] AS [t1]
WHERE ([t0].[ListPrice] > @p0) AND ([t1].[ListPrice] < @p1)',N'@p0 decimal(33,4),@p1 decimal(33,4)',@p0=2000.0000,@p1=100.0000

-- CrossJoinWithJoin
exec sp_executesql N'SELECT [t0].[Name] AS [Expensive], [t1].[Name] AS [Cheap]
FROM [Production].[Product] AS [t0]
INNER JOIN [Production].[Product] AS [t1] ON 1 = 1
WHERE ([t0].[ListPrice] > @p0) AND ([t1].[ListPrice] < @p1)',N'@p0 decimal(33,4),@p1 decimal(33,4)',@p0=2000.0000,@p1=100.0000

-- SelfJoin
SELECT [t0].[Name], [t0].[ListPrice], [t1].[Name] AS [Name2], (
    SELECT COUNT(*)
    FROM [Production].[Product] AS [t2]
    WHERE ([t2].[ProductID] <> [t0].[ProductID]) AND ([t0].[ListPrice] = [t2].[ListPrice])
    ) AS [value]
FROM [Production].[Product] AS [t0]
LEFT OUTER JOIN [Production].[Product] AS [t1] ON ([t1].[ProductID] <> [t0].[ProductID]) AND ([t0].[ListPrice] = [t1].[ListPrice])
ORDER BY [t0].[ProductID], [t1].[ProductID]

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
SELECT [t1].[ProductCategoryID], [t3].[ProductCategoryID] AS [ProductCategoryID2], [t3].[ProductSubcategoryID], [t3].[Name]
FROM (
    SELECT [t0].[ProductCategoryID]
    FROM [Production].[ProductSubcategory] AS [t0]
    GROUP BY [t0].[ProductCategoryID]
    ) AS [t1]
CROSS APPLY (
    SELECT TOP (1) [t2].[ProductCategoryID], [t2].[ProductSubcategoryID], [t2].[Name]
    FROM [Production].[ProductSubcategory] AS [t2]
    WHERE [t1].[ProductCategoryID] = [t2].[ProductCategoryID]
    ) AS [t3]

-- CrossApplyWithGroupJoinAndTake
SELECT [t0].[ProductCategoryID], [t0].[Name], [t2].[ProductCategoryID] AS [ProductCategoryID2], [t2].[ProductSubcategoryID], [t2].[Name] AS [Name2]
FROM [Production].[ProductCategory] AS [t0]
CROSS APPLY (
    SELECT TOP (1) [t1].[ProductCategoryID], [t1].[ProductSubcategoryID], [t1].[Name]
    FROM [Production].[ProductSubcategory] AS [t1]
    WHERE [t0].[ProductCategoryID] = [t1].[ProductCategoryID]
    ) AS [t2]

-- CrossApplyWithAssociationAndTake
SELECT [t0].[ProductCategoryID], [t0].[Name], [t2].[ProductCategoryID] AS [ProductCategoryID2], [t2].[ProductSubcategoryID], [t2].[Name] AS [Name2]
FROM [Production].[ProductCategory] AS [t0]
CROSS APPLY (
    SELECT TOP (1) [t1].[ProductCategoryID], [t1].[ProductSubcategoryID], [t1].[Name]
    FROM [Production].[ProductSubcategory] AS [t1]
    WHERE [t1].[ProductCategoryID] = [t0].[ProductCategoryID]
    ) AS [t2]

-- OuterApplyWithGroupByAndFirstOrDefault
SELECT [t0].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [t0]
GROUP BY [t0].[ProductCategoryID]

exec sp_executesql N'SELECT TOP (1) [t0].[ProductCategoryID], [t0].[ProductSubcategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE @x1 = [t0].[ProductCategoryID]',N'@x1 int',@x1=1

exec sp_executesql N'SELECT TOP (1) [t0].[ProductCategoryID], [t0].[ProductSubcategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE @x1 = [t0].[ProductCategoryID]',N'@x1 int',@x1=2

exec sp_executesql N'SELECT TOP (1) [t0].[ProductCategoryID], [t0].[ProductSubcategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE @x1 = [t0].[ProductCategoryID]',N'@x1 int',@x1=3

exec sp_executesql N'SELECT TOP (1) [t0].[ProductCategoryID], [t0].[ProductSubcategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE @x1 = [t0].[ProductCategoryID]',N'@x1 int',@x1=4

-- OuterApplyWithGroupJoinAndFirstOrDefault
SELECT [t0].[ProductCategoryID], [t0].[Name]
FROM [Production].[ProductCategory] AS [t0]

exec sp_executesql N'SELECT TOP (1) [t0].[ProductCategoryID], [t0].[ProductSubcategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE @x1 = [t0].[ProductCategoryID]',N'@x1 int',@x1=4

exec sp_executesql N'SELECT TOP (1) [t0].[ProductCategoryID], [t0].[ProductSubcategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE @x1 = [t0].[ProductCategoryID]',N'@x1 int',@x1=1

exec sp_executesql N'SELECT TOP (1) [t0].[ProductCategoryID], [t0].[ProductSubcategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE @x1 = [t0].[ProductCategoryID]',N'@x1 int',@x1=3

exec sp_executesql N'SELECT TOP (1) [t0].[ProductCategoryID], [t0].[ProductSubcategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE @x1 = [t0].[ProductCategoryID]',N'@x1 int',@x1=2

-- OuterApplyWithAssociationAndFirstOrDefault
SELECT [t0].[ProductCategoryID], [t0].[Name]
FROM [Production].[ProductCategory] AS [t0]

exec sp_executesql N'SELECT TOP (1) [t0].[ProductCategoryID], [t0].[ProductSubcategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE [t0].[ProductCategoryID] = @x1',N'@x1 int',@x1=4

exec sp_executesql N'SELECT TOP (1) [t0].[ProductCategoryID], [t0].[ProductSubcategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE [t0].[ProductCategoryID] = @x1',N'@x1 int',@x1=1

exec sp_executesql N'SELECT TOP (1) [t0].[ProductCategoryID], [t0].[ProductSubcategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE [t0].[ProductCategoryID] = @x1',N'@x1 int',@x1=3

exec sp_executesql N'SELECT TOP (1) [t0].[ProductCategoryID], [t0].[ProductSubcategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
WHERE [t0].[ProductCategoryID] = @x1',N'@x1 int',@x1=2

-- Concat
exec sp_executesql N'SELECT [t2].[Name]
FROM (
    SELECT [t0].[Name]
    FROM [Production].[Product] AS [t0]
    WHERE [t0].[ListPrice] < @p0
    UNION ALL
    SELECT [t1].[Name]
    FROM [Production].[Product] AS [t1]
    WHERE [t1].[ListPrice] > @p1
    ) AS [t2]',N'@p0 decimal(33,4),@p1 decimal(33,4)',@p0=100.0000,@p1=2000.0000

-- ConcatWithSelect
exec sp_executesql N'SELECT [t2].[Name]
FROM (
    SELECT [t0].[Style], [t0].[RowVersion], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
    FROM [Production].[Product] AS [t0]
    WHERE [t0].[ListPrice] < @p0
    UNION ALL
    SELECT [t1].[Style], [t1].[RowVersion], [t1].[ProductSubcategoryID], [t1].[ProductID], [t1].[Name], [t1].[ListPrice]
    FROM [Production].[Product] AS [t1]
    WHERE [t1].[ListPrice] > @p1
    ) AS [t2]',N'@p0 decimal(33,4),@p1 decimal(33,4)',@p0=100.0000,@p1=2000.0000

-- Distinct
SELECT DISTINCT [t0].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [t0]

-- DistinctWithGroupBy
SELECT [t0].[ProductCategoryID]
FROM [Production].[ProductSubcategory] AS [t0]
GROUP BY [t0].[ProductCategoryID]

-- DistinctMultipleKeys
SELECT DISTINCT [t0].[ProductCategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]

-- DistinctMultipleKeysWithGroupBy
SELECT [t0].[ProductCategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
GROUP BY [t0].[ProductCategoryID], [t0].[Name]

-- DistinctWithGroupByAndSelectAndFirstOrDefault
SELECT (
    SELECT TOP (1) [t2].[Name]
    FROM [Production].[ProductSubcategory] AS [t2]
    WHERE [t1].[ProductCategoryID] = [t2].[ProductCategoryID]
    ) AS [value]
FROM (
    SELECT [t0].[ProductCategoryID]
    FROM [Production].[ProductSubcategory] AS [t0]
    GROUP BY [t0].[ProductCategoryID]
    ) AS [t1]

-- Intersect
exec sp_executesql N'SELECT DISTINCT [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE (EXISTS(
    SELECT NULL AS [EMPTY]
    FROM [Production].[Product] AS [t1]
    WHERE ((([t0].[Name] IS NULL) AND ([t1].[Name] IS NULL)) OR (([t0].[Name] IS NOT NULL) AND ([t1].[Name] IS NOT NULL) AND ((([t0].[Name] IS NULL) AND ([t1].[Name] IS NULL)) OR (([t0].[Name] IS NOT NULL) AND ([t1].[Name] IS NOT NULL) AND ([t0].[Name] = [t1].[Name]))))) AND ([t0].[ListPrice] = [t1].[ListPrice]) AND ([t1].[ListPrice] < @p0)
    )) AND ([t0].[ListPrice] > @p1)',N'@p0 decimal(33,4),@p1 decimal(33,4)',@p0=2000.0000,@p1=100.0000
    
-- Except
exec sp_executesql N'SELECT DISTINCT [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE (NOT (EXISTS(
    SELECT NULL AS [EMPTY]
    FROM [Production].[Product] AS [t1]
    WHERE ((([t0].[Name] IS NULL) AND ([t1].[Name] IS NULL)) OR (([t0].[Name] IS NOT NULL) AND ([t1].[Name] IS NOT NULL) AND ((([t0].[Name] IS NULL) AND ([t1].[Name] IS NULL)) OR (([t0].[Name] IS NOT NULL) AND ([t1].[Name] IS NOT NULL) AND ([t0].[Name] = [t1].[Name]))))) AND ([t0].[ListPrice] = [t1].[ListPrice]) AND ([t1].[ListPrice] > @p0)
    ))) AND ([t0].[ListPrice] > @p1)',N'@p0 decimal(33,4),@p1 decimal(33,4)',@p0=2000.0000,@p1=100.0000

-- Skip
exec sp_executesql N'SELECT [t2].[Name]
FROM (
    SELECT [t1].[Name], [t1].[ROW_NUMBER]
    FROM (
        SELECT ROW_NUMBER() OVER (ORDER BY [t0].[Style], [t0].[RowVersion], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]) AS [ROW_NUMBER], [t0].[Name]
        FROM [Production].[Product] AS [t0]
        ) AS [t1]
    WHERE [t1].[ROW_NUMBER] > @p0
    ) AS [t2]
ORDER BY [t2].[ROW_NUMBER]',N'@p0 int',@p0=10

-- OrderByAndSkip
exec sp_executesql N'SELECT [t2].[Name]
FROM (
    SELECT [t1].[Name], [t1].[ROW_NUMBER]
    FROM (
        SELECT ROW_NUMBER() OVER (ORDER BY [t0].[Name]) AS [ROW_NUMBER], [t0].[Name]
        FROM [Production].[Product] AS [t0]
        ) AS [t1]
    WHERE [t1].[ROW_NUMBER] > @p0
    ) AS [t2]
ORDER BY [t2].[ROW_NUMBER]',N'@p0 int',@p0=10

-- Take
SELECT TOP (10) [t0].[Name]
FROM [Production].[Product] AS [t0]

-- OrderByAndSkipAndTake
exec sp_executesql N'SELECT [t2].[Name]
FROM (
    SELECT [t1].[Name], [t1].[ROW_NUMBER]
    FROM (
        SELECT ROW_NUMBER() OVER (ORDER BY [t0].[Name]) AS [ROW_NUMBER], [t0].[Name]
        FROM [Production].[Product] AS [t0]
        ) AS [t1]
    WHERE [t1].[ROW_NUMBER] BETWEEN @p0 + 1 AND @p0 + @p1
    ) AS [t2]
ORDER BY [t2].[ROW_NUMBER]',N'@p0 int,@p1 int',@p0=20,@p1=10

-- OrderBy
SELECT [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
ORDER BY [t0].[ListPrice]

-- OrderByDescending
SELECT [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
ORDER BY [t0].[ListPrice] DESC

-- OrderByAndThenBy
SELECT [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
ORDER BY [t0].[ListPrice], [t0].[Name]

-- OrderByAnonymousType
-- InvalidOperationException.

-- OrderByAndOrderBy
SELECT [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
ORDER BY [t0].[Name], [t0].[ListPrice]

-- Reverse
-- NotSupportedException.

-- CastPrimitive
-- NotSupportedException.

-- CastEntity
exec sp_executesql N'SELECT [t0].[Style], [t0].[RowVersion], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[Name] LIKE @p0',N'@p0 nvarchar(4000)',@p0=N'Road-750%'

-- AsEnumerableAsQueryable
exec sp_executesql N'SELECT [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ListPrice] > @p0',N'@p0 decimal(33,4)',@p0=0.0000

SELECT [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]

-- SelectEntities
-- N/A

-- SelectEntityObjects
exec sp_executesql N'SELECT [t0].[Style], [t0].[RowVersion], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE (([t0].[Style] <> @p0) AND ([t0].[Style] <> @p1)) OR ([t0].[Style] IS NULL)',N'@p0 nchar(2),@p1 nchar(2)',@p0=N'W ',@p1=N'M '

-- First
SELECT TOP (1) [t0].[Name]
FROM [Production].[Product] AS [t0]

-- FirstOrDefault
exec sp_executesql N'SELECT TOP (1) [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ListPrice] > @p0',N'@p0 decimal(33,4)',@p0=5000.0000

-- Single
exec sp_executesql N'SELECT [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ListPrice] < @p0',N'@p0 decimal(33,4)',@p0=50.0000

-- SingleOrDefault
exec sp_executesql N'SELECT [t1].[ListPrice], [t1].[value] AS [Count]
FROM (
    SELECT COUNT(*) AS [value], [t0].[ListPrice]
    FROM [Production].[Product] AS [t0]
    GROUP BY [t0].[ListPrice]
    ) AS [t1]
WHERE [t1].[value] > @p0',N'@p0 int',@p0=10

-- Count
SELECT COUNT(*) AS [value]
FROM [Production].[ProductCategory] AS [t0]

-- LongCount
exec sp_executesql N'SELECT COUNT_BIG(*) AS [value]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ListPrice] > @p0',N'@p0 decimal(33,4)',@p0=0.0000

-- Max
SELECT MAX([t0].[ModifiedDate]) AS [value]
FROM [Production].[ProductPhoto] AS [t0]

-- Min
SELECT MIN([t0].[ListPrice]) AS [value]
FROM [Production].[Product] AS [t0]

-- Average
SELECT AVG([t0].[ListPrice]) AS [value]
FROM [Production].[Product] AS [t0]

-- Sum
SELECT SUM([t0].[ListPrice]) AS [value]
FROM [Production].[Product] AS [t0]

-- Any
exec sp_executesql N'SELECT 
    (CASE 
        WHEN EXISTS(
            SELECT NULL AS [EMPTY]
            FROM [Production].[Product] AS [t0]
            ) THEN 1
        ELSE 0
     END) AS [value]',N'@p0 nchar(2),@p1 nchar(2),@p2 nchar(2)',@p0=N'U ',@p1=N'W ',@p2=N'M '

-- Contains
exec sp_executesql N'SELECT 
    (CASE 
        WHEN EXISTS(
            SELECT NULL AS [EMPTY]
            FROM [Production].[Product] AS [t0]
            WHERE [t0].[ListPrice] = @p0
            ) THEN 1
        ELSE 0
     END) AS [value]',N'@p0 decimal(33,4),@p1 nchar(2),@p2 nchar(2),@p3 nchar(2)',@p0=100.0000,@p1=N'U ',@p2=N'W ',@p3=N'M '

-- AnyWithPredicate
exec sp_executesql N'SELECT 
    (CASE 
        WHEN EXISTS(
            SELECT NULL AS [EMPTY]
            FROM [Production].[Product] AS [t0]
            WHERE [t0].[ListPrice] = @p0
            ) THEN 1
        ELSE 0
     END) AS [value]',N'@p0 decimal(33,4)',@p0=100.0000

-- AllNot
exec sp_executesql N'SELECT 
    (CASE 
        WHEN NOT (EXISTS(
            SELECT NULL AS [EMPTY]
            FROM [Production].[Product] AS [t1]
            WHERE NOT ([t1].[ProductSubcategoryID] IS NOT NULL)
            )) THEN 1
        WHEN NOT NOT (EXISTS(
            SELECT NULL AS [EMPTY]
            FROM [Production].[Product] AS [t1]
            WHERE NOT ([t1].[ProductSubcategoryID] IS NOT NULL)
            )) THEN 0
        ELSE NULL
     END) AS [value]',N'@p0 nchar(2),@p1 nchar(2),@p2 nchar(2)',@p0=N'U ',@p1=N'W ',@p2=N'M '

-- NotAny
exec sp_executesql N'SELECT 
    (CASE 
        WHEN EXISTS(
            SELECT NULL AS [EMPTY]
            FROM [Production].[Product] AS [t0]
            WHERE NOT ([t0].[ProductSubcategoryID] IS NOT NULL)
            ) THEN 1
        ELSE 0
     END) AS [value]',N'@p0 nchar(2),@p1 nchar(2),@p2 nchar(2)',@p0=N'U ',@p1=N'W ',@p2=N'M '
