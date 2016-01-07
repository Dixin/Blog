-- DefaultIfEmpty
SELECT [t2].[test], [t2].[Style], [t2].[ProductSubcategoryID], [t2].[ProductID], [t2].[Name], [t2].[ListPrice]
FROM (
    SELECT NULL AS [EMPTY]
    ) AS [t0]
LEFT OUTER JOIN (
    SELECT 1 AS [test], [t1].[Style], [t1].[ProductSubcategoryID], [t1].[ProductID], [t1].[Name], [t1].[ListPrice]
    FROM [Production].[Product] AS [t1]
    ) AS [t2] ON 1=1 

-- Where
exec sp_executesql N'SELECT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ListPrice] > @p0',N'@p0 decimal(33,4)',@p0=100.0000

-- WhereWithOr
exec sp_executesql N'SELECT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE ([t0].[ListPrice] < @p0) OR ([t0].[ListPrice] > @p1)',N'@p0 decimal(33,4),@p1 decimal(33,4)',@p0=100.0000,@p1=200.0000

-- WhereWithAnd
exec sp_executesql N'SELECT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE ([t0].[ListPrice] > @p0) AND ([t0].[ListPrice] < @p1)',N'@p0 decimal(33,4),@p1 decimal(33,4)',@p0=100.0000,@p1=200.0000

-- WhereAndWhere
exec sp_executesql N'SELECT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE ([t0].[ListPrice] < @p0) AND ([t0].[ListPrice] > @p1)',N'@p0 decimal(33,4),@p1 decimal(33,4)',@p0=200.0000,@p1=100.0000

-- WhereWithLike
exec sp_executesql N'SELECT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[Name] LIKE @p0',N'@p0 nvarchar(4000)',@p0=N'ML %'

-- WhereWithLikeMethod
exec sp_executesql N'SELECT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[Name] LIKE @p0',N'@p0 nvarchar(4000)',@p0=N'%Mountain%'

-- WhereWithContains
exec sp_executesql N'SELECT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[Name] IN (@p0, @p1, @p2)',N'@p0 nvarchar(4000),@p1 nvarchar(4000),@p2 nvarchar(4000)',@p0=N'Blade',@p1=N'Chainring',@p2=N'Freewheel'

-- WhereWithNull
SELECT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ProductSubcategoryID] IS NOT NULL

-- OfType
exec sp_executesql N'SELECT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE (([t0].[Style] <> @p0) AND ([t0].[Style] <> @p1)) OR ([t0].[Style] IS NULL)',N'@p0 nchar(2),@p1 nchar(2)',@p0=N'W ',@p1=N'M '

-- Select
exec sp_executesql N'SELECT ((CONVERT(NVarChar,[t0].[ProductID])) + @p1) + [t0].[Name] AS [value]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ListPrice] > @p0',N'@p0 decimal(33,4),@p1 nvarchar(4000)',@p0=100.0000,@p1=N': '

-- SelectWithStringConcat
exec sp_executesql N'SELECT ((CONVERT(NVarChar,[t0].[ProductID])) + @p1) + [t0].[Name] AS [value]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ListPrice] > @p0',N'@p0 decimal(33,4),@p1 nvarchar(4000)',@p0=100.0000,@p1=N': '

-- SelectAnonymousType
exec sp_executesql N'SELECT [t0].[ProductID] AS [Id], [t0].[Name]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ListPrice] > @p0',N'@p0 decimal(33,4)',@p0=100.0000

-- SelectEntity
-- NotSupportedException.

-- SelectEntityObjects
exec sp_executesql N'SELECT [t0].[Name]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ListPrice] > @p0',N'@p0 decimal(33,4)',@p0=100.0000

-- SelectWithCase
exec sp_executesql N'SELECT [t0].[Name], 
    (CASE 
        WHEN [t0].[ListPrice] > @p0 THEN 1
        WHEN NOT ([t0].[ListPrice] > @p0) THEN 0
        ELSE NULL
     END) AS [HasListPrice]
FROM [Production].[Product] AS [t0]',N'@p0 decimal(33,4)',@p0=0

-- Grouping
exec sp_executesql N'SELECT [t1].[value] AS [Key]
FROM (
    SELECT SUBSTRING([t0].[Name], @p0 + 1, @p1) AS [value]
    FROM [Production].[Product] AS [t0]
    ) AS [t1]
GROUP BY [t1].[value]',N'@p0 int,@p1 int',@p0=0,@p1=1

exec sp_executesql N'SELECT [t0].[Name]
FROM [Production].[Product] AS [t0]
WHERE ((@x1 IS NULL) AND (SUBSTRING([t0].[Name], @p0 + 1, @p1) IS NULL)) OR ((@x1 IS NOT NULL) AND (SUBSTRING([t0].[Name], @p0 + 1, @p1) IS NOT NULL) AND (@x1 = SUBSTRING([t0].[Name], @p0 + 1, @p1)))',N'@p0 int,@p1 int,@x1 nvarchar(4000)',@p0=0,@p1=1,@x1=N'A'

exec sp_executesql N'SELECT [t0].[Name]
FROM [Production].[Product] AS [t0]
WHERE ((@x1 IS NULL) AND (SUBSTRING([t0].[Name], @p0 + 1, @p1) IS NULL)) OR ((@x1 IS NOT NULL) AND (SUBSTRING([t0].[Name], @p0 + 1, @p1) IS NOT NULL) AND (@x1 = SUBSTRING([t0].[Name], @p0 + 1, @p1)))',N'@p0 int,@p1 int,@x1 nvarchar(4000)',@p0=0,@p1=1,@x1=N'B'

-- ...

exec sp_executesql N'SELECT [t0].[Name]
FROM [Production].[Product] AS [t0]
WHERE ((@x1 IS NULL) AND (SUBSTRING([t0].[Name], @p0 + 1, @p1) IS NULL)) OR ((@x1 IS NOT NULL) AND (SUBSTRING([t0].[Name], @p0 + 1, @p1) IS NOT NULL) AND (@x1 = SUBSTRING([t0].[Name], @p0 + 1, @p1)))',N'@p0 int,@p1 int,@x1 nvarchar(4000)',@p0=0,@p1=1,@x1=N'T'

-- GroupBy
exec sp_executesql N'SELECT COUNT(*) AS [Count], [t1].[value] AS [Key]
FROM (
    SELECT SUBSTRING([t0].[Name], @p0 + 1, @p1) AS [value]
    FROM [Production].[Product] AS [t0]
    ) AS [t1]
GROUP BY [t1].[value]',N'@p0 int,@p1 int',@p0=0,@p1=1

-- GroupByWithWhere
exec sp_executesql N'SELECT [t2].[value2] AS [Key], [t2].[value] AS [Count]
FROM (
    SELECT COUNT(*) AS [value], [t1].[value] AS [value2]
    FROM (
        SELECT SUBSTRING([t0].[Name], @p0 + 1, @p1) AS [value]
        FROM [Production].[Product] AS [t0]
        ) AS [t1]
    GROUP BY [t1].[value]
    ) AS [t2]
WHERE [t2].[value] > @p2',N'@p0 int,@p1 int,@p2 int',@p0=0,@p1=1,@p2=0

-- InnerJoin
SELECT [t0].[Name] AS [Subcategory], [t1].[Name] AS [Category]
FROM [Production].[ProductSubcategory] AS [t0]
INNER JOIN [Production].[ProductCategory] AS [t1] ON [t0].[ProductCategoryID] = ([t1].[ProductCategoryID])

-- InnerJoinWithSelectMany
SELECT [t0].[Name] AS [Subcategory], [t1].[Name] AS [Category]
FROM [Production].[ProductSubcategory] AS [t0], [Production].[ProductCategory] AS [t1]
WHERE [t0].[ProductCategoryID] = ([t1].[ProductCategoryID])

-- InnerJoinWithAssociation
SELECT [t0].[Name] AS [Subcategory], [t1].[Name] AS [Category]
FROM [Production].[ProductSubcategory] AS [t0]
INNER JOIN [Production].[ProductCategory] AS [t1] ON [t0].[ProductCategoryID] = ([t1].[ProductCategoryID])

-- InnerJoinWithMultipleKeys
exec sp_executesql N'SELECT [t0].[Name] AS [Subcategory], [t1].[Name] AS [Category]
FROM [Production].[ProductSubcategory] AS [t0]
INNER JOIN [Production].[ProductCategory] AS [t1] ON ((COALESCE([t0].[ProductCategoryID],@p0)) = [t1].[ProductCategoryID]) AND (SUBSTRING([t0].[Name], @p1 + 1, @p2) = SUBSTRING([t1].[Name], @p3 + 1, @p4))',N'@p0 int,@p1 int,@p2 int,@p3 int,@p4 int',@p0=-1,@p1=0,@p2=1,@p3=0,@p4=1

-- LeftOuterJoin
SELECT [t0].[Name] AS [Subcategory], [t1].[Name], (
    SELECT COUNT(*)
    FROM [Production].[ProductCategory] AS [t2]
    WHERE [t0].[ProductCategoryID] = ([t2].[ProductCategoryID])
    ) AS [value]
FROM [Production].[ProductSubcategory] AS [t0]
LEFT OUTER JOIN [Production].[ProductCategory] AS [t1] ON [t0].[ProductCategoryID] = ([t1].[ProductCategoryID])
ORDER BY [t0].[ProductSubcategoryID], [t1].[ProductCategoryID]

-- LeftOuterJoinWithDefaultIfEmpty
SELECT [t0].[Name] AS [Subcategory], [t1].[Name] AS [Category]
FROM [Production].[ProductSubcategory] AS [t0]
LEFT OUTER JOIN [Production].[ProductCategory] AS [t1] ON [t0].[ProductCategoryID] = ([t1].[ProductCategoryID])

-- LeftOuterJoinWithSelect
SELECT [t0].[Name] AS [Subcategory], [t1].[Name], (
    SELECT COUNT(*)
    FROM [Production].[ProductCategory] AS [t2]
    WHERE [t0].[ProductCategoryID] = ([t2].[ProductCategoryID])
    ) AS [value]
FROM [Production].[ProductSubcategory] AS [t0]
LEFT OUTER JOIN [Production].[ProductCategory] AS [t1] ON [t0].[ProductCategoryID] = ([t1].[ProductCategoryID])
ORDER BY [t0].[ProductSubcategoryID], [t1].[ProductCategoryID]

-- LeftOuterJoinWithAssociation
SELECT [t0].[Name] AS [Subcategory], [t1].[Name] AS [Category]
FROM [Production].[ProductSubcategory] AS [t0]
LEFT OUTER JOIN [Production].[ProductCategory] AS [t1] ON [t1].[ProductCategoryID] = [t0].[ProductCategoryID]

-- CrossJoin
SELECT [t0].[Name] AS [Product], [t2].[LargePhotoFileName] AS [Photo]
FROM [Production].[Product] AS [t0]
CROSS JOIN [Production].[ProductProductPhoto] AS [t1]
INNER JOIN [Production].[ProductPhoto] AS [t2] ON [t2].[ProductPhotoID] = [t1].[ProductPhotoID]
WHERE [t1].[ProductID] = [t0].[ProductID]

-- CrossJoinWithSelectMany
SELECT [t0].[Name] AS [Subcategory], [t1].[Name] AS [Category]
FROM [Production].[ProductSubcategory] AS [t0], [Production].[ProductCategory] AS [t1]

-- CrossJoinWithJoin
SELECT [t0].[Name] AS [Subcategory], [t1].[Name] AS [Category]
FROM [Production].[ProductSubcategory] AS [t0]
INNER JOIN [Production].[ProductCategory] AS [t1] ON 1 = 1

-- SelfJoin
exec sp_executesql N'SELECT [t0].[Name] AS [Product], [t0].[ListPrice], [t1].[Name], (
    SELECT COUNT(*)
    FROM [Production].[Product] AS [t2]
    WHERE ([t2].[ProductID] <> [t0].[ProductID]) AND ([t0].[ListPrice] = [t2].[ListPrice])
    ) AS [value]
FROM [Production].[Product] AS [t0]
LEFT OUTER JOIN [Production].[Product] AS [t1] ON ([t1].[ProductID] <> [t0].[ProductID]) AND ([t0].[ListPrice] = [t1].[ListPrice])
WHERE [t0].[ListPrice] > @p0
ORDER BY [t0].[ProductID], [t1].[ProductID]',N'@p0 decimal(33,4)',@p0=0

-- Concat
exec sp_executesql N'SELECT [t2].[Name], [t2].[ListPrice]
FROM (
    SELECT [t0].[Name], [t0].[ListPrice]
    FROM [Production].[Product] AS [t0]
    WHERE [t0].[ListPrice] < @p0
    UNION ALL
    SELECT [t1].[Name], [t1].[ListPrice]
    FROM [Production].[Product] AS [t1]
    WHERE [t1].[ListPrice] > @p1
    ) AS [t2]',N'@p0 decimal(33,4),@p1 decimal(33,4)',@p0=100.0000,@p1=200.0000

-- ConcatWithSelect
exec sp_executesql N'SELECT [t2].[Name], [t2].[ListPrice]
FROM (
    SELECT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
    FROM [Production].[Product] AS [t0]
    WHERE [t0].[ListPrice] < @p0
    UNION ALL
    SELECT [t1].[Style], [t1].[ProductSubcategoryID], [t1].[ProductID], [t1].[Name], [t1].[ListPrice]
    FROM [Production].[Product] AS [t1]
    WHERE [t1].[ListPrice] > @p1
    ) AS [t2]',N'@p0 decimal(33,4),@p1 decimal(33,4)',@p0=100.0000,@p1=200.0000

-- Distinct
SELECT DISTINCT [t0].[ProductSubcategoryID]
FROM [Production].[Product] AS [t0]

-- DistinctWithGroupByAndSelect
SELECT [t3].[test], [t3].[ProductSubcategoryID] AS [Subcategory], [t3].[Name] AS [Product]
FROM (
    SELECT [t0].[ProductSubcategoryID]
    FROM [Production].[Product] AS [t0]
    GROUP BY [t0].[ProductSubcategoryID]
    ) AS [t1]
OUTER APPLY (
    SELECT TOP (1) 1 AS [test], [t2].[ProductSubcategoryID], [t2].[Name]
    FROM [Production].[Product] AS [t2]
    WHERE (([t1].[ProductSubcategoryID] IS NULL) AND ([t2].[ProductSubcategoryID] IS NULL)) OR (([t1].[ProductSubcategoryID] IS NOT NULL) AND ([t2].[ProductSubcategoryID] IS NOT NULL) AND ([t1].[ProductSubcategoryID] = [t2].[ProductSubcategoryID]))
    ) AS [t3]

-- DistinctWithGroupByAndSelectMany
SELECT [t3].[ProductSubcategoryID] AS [Subcategory], [t3].[Name] AS [Product]
FROM (
    SELECT [t0].[ProductSubcategoryID]
    FROM [Production].[Product] AS [t0]
    GROUP BY [t0].[ProductSubcategoryID]
    ) AS [t1]
CROSS APPLY (
    SELECT TOP (1) [t2].[ProductSubcategoryID], [t2].[Name]
    FROM [Production].[Product] AS [t2]
    WHERE (([t1].[ProductSubcategoryID] IS NULL) AND ([t2].[ProductSubcategoryID] IS NULL)) OR (([t1].[ProductSubcategoryID] IS NOT NULL) AND ([t2].[ProductSubcategoryID] IS NOT NULL) AND ([t1].[ProductSubcategoryID] = [t2].[ProductSubcategoryID]))
    ) AS [t3]

-- Intersect
exec sp_executesql N'SELECT DISTINCT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE (EXISTS(
    SELECT NULL AS [EMPTY]
    FROM [Production].[Product] AS [t1]
    WHERE ([t0].[ProductID] = [t1].[ProductID]) AND ([t1].[ListPrice] < @p0)
    )) AND ([t0].[ListPrice] > @p1)',N'@p0 decimal(33,4),@p1 decimal(33,4)',@p0=200.0000,@p1=100.0000
	
-- Except
exec sp_executesql N'SELECT DISTINCT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE (NOT (EXISTS(
    SELECT NULL AS [EMPTY]
    FROM [Production].[Product] AS [t1]
    WHERE ([t0].[ProductID] = [t1].[ProductID]) AND ([t1].[ListPrice] > @p0)
    ))) AND ([t0].[ListPrice] > @p1)',N'@p0 decimal(33,4),@p1 decimal(33,4)',@p0=200.0000,@p1=100.0000

-- Skip
exec sp_executesql N'SELECT [t2].[Name]
FROM (
    SELECT [t1].[Name], [t1].[ROW_NUMBER]
    FROM (
        SELECT ROW_NUMBER() OVER (ORDER BY [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]) AS [ROW_NUMBER], [t0].[Name]
        FROM [Production].[Product] AS [t0]
        ) AS [t1]
    WHERE [t1].[ROW_NUMBER] > @p0
    ) AS [t2]
ORDER BY [t2].[ROW_NUMBER]',N'@p0 int',@p0=10

-- OrderBySkip
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

-- OrderBySkipTake
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

-- OrderByThenBy
SELECT [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
ORDER BY [t0].[ListPrice], [t0].[Name]

-- OrderByOrderBy
SELECT [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
ORDER BY [t0].[Name], [t0].[ListPrice]

-- Reverse
-- NotSupportedException.

-- Cast
exec sp_executesql N'SELECT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[Name] LIKE @p0',N'@p0 nvarchar(4000)',@p0=N'Road-750%'

-- First
SELECT TOP (1) [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]

-- FirstOrDefault
exec sp_executesql N'SELECT TOP (1) [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ListPrice] = @p0',N'@p0 decimal(33,4)',@p0=1.0000

-- Single
exec sp_executesql N'SELECT [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ListPrice] = @p0',N'@p0 decimal(31,4)',@p0=539.9900

-- SingleOrDefault
exec sp_executesql N'SELECT [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ListPrice] = @p0',N'@p0 decimal(33,4)',@p0=540.0000

-- Count
exec sp_executesql N'SELECT COUNT(*) AS [value]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ListPrice] = @p0',N'@p0 decimal(33,4)',@p0=0

-- LongCount
SELECT COUNT_BIG(*) AS [value]
FROM [Production].[Product] AS [t0]

-- Min
exec sp_executesql N'SELECT MIN([t0].[ListPrice]) AS [value]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ListPrice] > @p0',N'@p0 decimal(33,4)',@p0=0

-- Max
exec sp_executesql N'SELECT MAX([t0].[ListPrice]) AS [value]
FROM [Production].[Product] AS [t0]
WHERE (([t0].[Style] <> @p0) AND ([t0].[Style] <> @p1)) OR ([t0].[Style] IS NULL)',N'@p0 nchar(2),@p1 nchar(2)',@p0=N'W ',@p1=N'M '

-- Sum
SELECT SUM([t0].[ListPrice]) AS [value]
FROM [Production].[Product] AS [t0]

-- Average
exec sp_executesql N'SELECT AVG([t0].[ListPrice]) AS [value]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ListPrice] > @p0',N'@p0 decimal(33,4)',@p0=0

-- All
exec sp_executesql N'SELECT 
    (CASE 
        WHEN NOT (EXISTS(
            SELECT NULL AS [EMPTY]
            FROM [Production].[Product] AS [t1]
            WHERE (
                (CASE 
                    WHEN [t1].[ListPrice] > @p0 THEN 1
                    ELSE 0
                 END)) = 0
            )) THEN 1
        WHEN NOT NOT (EXISTS(
            SELECT NULL AS [EMPTY]
            FROM [Production].[Product] AS [t1]
            WHERE (
                (CASE 
                    WHEN [t1].[ListPrice] > @p0 THEN 1
                    ELSE 0
                 END)) = 0
            )) THEN 0
        ELSE NULL
     END) AS [value]',N'@p0 decimal(33,4),@p1 nchar(2),@p2 nchar(2),@p3 nchar(2)',@p0=0,@p1=N'U ',@p2=N'W ',@p3=N'M '

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
     END) AS [value]',N'@p0 decimal(31,4)',@p0=9.9900
