-- Stored procedure with single result type.
exec [dbo].[uspGetManagerEmployees] @BusinessEntityID=2

-- Stored procedure with output parameter.
declare @p1 int
set @p1=0
exec [dbo].[uspLogError] @ErrorLogID=@p1 output
select @p1

-- Stored procedure with multiple result types.
exec [dbo].[uspGetCategoryAndSubCategory] @CategoryID=1

-- Table-valued function.
exec sp_executesql N'SELECT TOP (2) 
    [top].[C1] AS [C1], 
    [top].[PersonID] AS [PersonID], 
    [top].[FirstName] AS [FirstName], 
    [top].[LastName] AS [LastName], 
    [top].[JobTitle] AS [JobTitle], 
    [top].[BusinessEntityType] AS [BusinessEntityType]
    FROM ( SELECT TOP (2) 
        [Extent1].[PersonID] AS [PersonID], 
        [Extent1].[FirstName] AS [FirstName], 
        [Extent1].[LastName] AS [LastName], 
        [Extent1].[JobTitle] AS [JobTitle], 
        [Extent1].[BusinessEntityType] AS [BusinessEntityType], 
        1 AS [C1]
        FROM [dbo].[ufnGetContactInformation](@PersonID) AS [Extent1]
    )  AS [top]',N'@PersonID int',@PersonID=1

-- Non-composable scalar-valued function.
exec sp_executesql N'SELECT [dbo].[ufnGetProductStandardCost](@ProductID, @OrderDate)',N'@ProductID int,@OrderDate datetime2(7)',@ProductID=999,@OrderDate='2015-12-28 02:22:53.0353800'

-- Composable scalar-valued function.
SELECT 
    CASE WHEN ( EXISTS (SELECT 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent1]
        WHERE [Extent1].[ListPrice] <= ([dbo].[ufnGetProductListPrice](999, SysDateTime()))
    )) THEN cast(1 as bit) WHEN ( NOT EXISTS (SELECT 
        1 AS [C1]
        FROM [Production].[Product] AS [Extent2]
        WHERE [Extent2].[ListPrice] <= ([dbo].[ufnGetProductListPrice](999, SysDateTime()))
    )) THEN cast(0 as bit) END AS [C1]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
	
-- Aggregate function
SELECT 
    1 AS [C1], 
    [GroupBy1].[K1] AS [ProductCategoryID], 
    [GroupBy1].[A1] AS [C2]
    FROM ( SELECT 
        [Extent1].[ProductCategoryID] AS [K1], 
        [dbo].[Concat]([Extent1].[Name]) AS [A1]
        FROM [Production].[ProductSubcategory] AS [Extent1]
        GROUP BY [Extent1].[ProductCategoryID]
    )  AS [GroupBy1]

-- Built-in function.
SELECT 
    1 AS [C1], 
    [GroupBy1].[K1] AS [ProductCategoryID], 
    [GroupBy1].[A1] AS [C2]
    FROM ( SELECT 
        [Extent1].[K1] AS [K1], 
        [dbo].[Concat]([Extent1].[A1]) AS [A1]
        FROM ( SELECT 
            [Extent1].[ProductCategoryID] AS [K1], 
            LEFT([Extent1].[Name], 4) AS [A1]
            FROM [Production].[ProductSubcategory] AS [Extent1]
        )  AS [Extent1]
        GROUP BY [K1]
    )  AS [GroupBy1]

-- Niladic function.
SELECT 
    [Limit1].[C2] AS [C1], 
    [Limit1].[ProductCategoryID] AS [ProductCategoryID], 
    [Limit1].[C1] AS [C2], 
    [Limit1].[C3] AS [C3], 
    [Limit1].[C4] AS [C4], 
    [Limit1].[C5] AS [C5], 
    [Limit1].[C6] AS [C6], 
    [Limit1].[C7] AS [C7]
    FROM ( SELECT TOP (1) 
        [GroupBy1].[A1] AS [C1], 
        [GroupBy1].[K1] AS [ProductCategoryID], 
        1 AS [C2], 
        CURRENT_TIMESTAMP AS [C3], 
        CURRENT_USER AS [C4], 
        SESSION_USER AS [C5], 
        SYSTEM_USER AS [C6], 
        USER AS [C7]
        FROM ( SELECT 
            [Extent1].[K1] AS [K1], 
            [dbo].[Concat]([Extent1].[A1]) AS [A1]
            FROM ( SELECT 
                [Extent1].[ProductCategoryID] AS [K1], 
                LEFT([Extent1].[Name], 4) AS [A1]
                FROM [Production].[ProductSubcategory] AS [Extent1]
            )  AS [Extent1]
            GROUP BY [K1]
        )  AS [GroupBy1]
    )  AS [Limit1]
