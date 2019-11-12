-- InlinePredicate
exec sp_executesql N'SELECT [t0].[Name]
FROM [Production].[Product] AS [t0]
WHERE ([t0].[ListPrice] > @p0) AND ([t0].[ProductSubcategoryID] IS NOT NULL)',N'@p0 decimal(33,4)',@p0=0

-- InlinePredicateCompiled
exec sp_executesql N'SELECT [t0].[Name]
FROM [Production].[Product] AS [t0]
WHERE ([t0].[ListPrice] > @p0) AND ([t0].[ProductSubcategoryID] IS NOT NULL)',N'@p0 decimal(33,4)',@p0=0

-- MethodPredicate
-- NotSupportedException

-- MethodPredicateCompiled
-- NotSupportedException

-- MethodSelector
exec sp_executesql N'SELECT [t0].[Name], [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ProductID] > @p0',N'@p0 int',@p0=100

-- LocalSelector
exec sp_executesql N'SELECT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ProductID] > @p0',N'@p0 int',@p0=100

-- RemoteMethod
exec sp_executesql N'SELECT [t0].[ModifiedDate]
FROM [Production].[ProductPhoto] AS [t0]
WHERE DATEDIFF(Year, [t0].[ModifiedDate], @p0) >= @p1',N'@p0 datetime,@p1 int',@p0='2016-02-07 22:35:33.983',@p1=5
