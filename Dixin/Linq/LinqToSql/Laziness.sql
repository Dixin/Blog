-- DeferredExecution
SELECT [t0].[ProductCategoryID], [t0].[ProductSubcategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]

exec sp_executesql N'SELECT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ProductSubcategoryID] = @p0',N'@p0 int',@p0=1

-- ...

exec sp_executesql N'SELECT [t0].[Style], [t0].[ProductSubcategoryID], [t0].[ProductID], [t0].[Name], [t0].[ListPrice]
FROM [Production].[Product] AS [t0]
WHERE [t0].[ProductSubcategoryID] = @p0',N'@p0 int',@p0=37

-- EagerLoadingWithSelect
SELECT [t1].[Name] AS [Category], [t0].[Name] AS [Subcategory], [t2].[Name], (
    SELECT COUNT(*)
    FROM [Production].[Product] AS [t3]
    WHERE [t3].[ProductSubcategoryID] = [t0].[ProductSubcategoryID]
    ) AS [value]
FROM [Production].[ProductSubcategory] AS [t0]
LEFT OUTER JOIN [Production].[ProductCategory] AS [t1] ON [t1].[ProductCategoryID] = [t0].[ProductCategoryID]
LEFT OUTER JOIN [Production].[Product] AS [t2] ON [t2].[ProductSubcategoryID] = [t0].[ProductSubcategoryID]
ORDER BY [t0].[ProductSubcategoryID], [t1].[ProductCategoryID], [t2].[ProductID]

-- EagerLoadingWithAssociation
SELECT [t0].[ProductCategoryID], [t2].[test], [t2].[ProductCategoryID] AS [ProductCategoryID2], [t2].[Name], [t3].[Style], [t3].[ProductSubcategoryID], [t3].[ProductID], [t3].[Name] AS [Name2], [t3].[ListPrice], (
    SELECT COUNT(*)
    FROM [Production].[Product] AS [t4]
    WHERE [t4].[ProductSubcategoryID] = [t0].[ProductSubcategoryID]
    ) AS [value], [t0].[ProductSubcategoryID] AS [ProductSubcategoryID2], [t0].[Name] AS [Name3]
FROM [Production].[ProductSubcategory] AS [t0]
LEFT OUTER JOIN (
    SELECT 1 AS [test], [t1].[ProductCategoryID], [t1].[Name]
    FROM [Production].[ProductCategory] AS [t1]
    ) AS [t2] ON [t2].[ProductCategoryID] = [t0].[ProductCategoryID]
LEFT OUTER JOIN [Production].[Product] AS [t3] ON [t3].[ProductSubcategoryID] = [t0].[ProductSubcategoryID]
ORDER BY [t0].[ProductSubcategoryID], [t2].[ProductCategoryID], [t3].[ProductID]

-- ConditionalEagerLoading
exec sp_executesql N'SELECT [t0].[ProductCategoryID], [t1].[Style], [t1].[ProductSubcategoryID], [t1].[ProductID], [t1].[Name], [t1].[ListPrice], (
    SELECT COUNT(*)
    FROM [Production].[Product] AS [t2]
    WHERE ([t2].[ListPrice] > @p0) AND ([t2].[ProductSubcategoryID] = ([t0].[ProductSubcategoryID]))
    ) AS [value], [t0].[ProductSubcategoryID] AS [ProductSubcategoryID2], [t0].[Name] AS [Name2]
FROM [Production].[ProductSubcategory] AS [t0]
LEFT OUTER JOIN [Production].[Product] AS [t1] ON ([t1].[ListPrice] > @p0) AND ([t1].[ProductSubcategoryID] = ([t0].[ProductSubcategoryID]))
ORDER BY [t0].[ProductSubcategoryID], [t1].[ProductID]',N'@p0 decimal(33,4)',@p0=0

-- DisableDeferredLoading
SELECT [t0].[ProductCategoryID], [t0].[ProductSubcategoryID], [t0].[Name]
FROM [Production].[ProductSubcategory] AS [t0]
