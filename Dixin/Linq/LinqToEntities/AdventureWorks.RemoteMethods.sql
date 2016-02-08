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

-- RemoteMethod
SELECT 
    [Extent1].[ModifiedDate] AS [ModifiedDate]
    FROM [Production].[ProductPhoto] AS [Extent1]
    WHERE (DATEDIFF (year, [Extent1].[ModifiedDate], SysDateTime())) >= 5
