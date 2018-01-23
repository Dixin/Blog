select cast(serverproperty('EngineEdition') as int)

SELECT Count(*)
FROM INFORMATION_SCHEMA.TABLES AS t
WHERE t.TABLE_SCHEMA + '.' + t.TABLE_NAME IN ('Production.vProductAndDescription','Production.ProductCategory','Production.ProductSubcategory','Production.Product','Production.ProductProductPhoto','Production.ProductPhoto')
    OR t.TABLE_NAME = 'EdmMetadata'

exec sp_executesql N'SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        COUNT(1) AS [A1]
        FROM [dbo].[__MigrationHistory] AS [Extent1]
        WHERE [Extent1].[ContextKey] = @p__linq__0
    )  AS [GroupBy1]',N'@p__linq__0 nvarchar(4000)',@p__linq__0=N'Dixin.Linq.EntityFramework.AdventureWorks'

SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        COUNT(1) AS [A1]
        FROM [dbo].[__MigrationHistory] AS [Extent1]
    )  AS [GroupBy1]

SELECT TOP (1) 
    [Extent1].[Id] AS [Id], 
    [Extent1].[ModelHash] AS [ModelHash]
    FROM [dbo].[EdmMetadata] AS [Extent1]
    ORDER BY [Extent1].[Id] DESC

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
