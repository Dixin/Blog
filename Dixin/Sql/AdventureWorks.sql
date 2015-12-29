-- Create stored procedure.
CREATE PROCEDURE [dbo].[uspGetCategoryAndSubCategory]
	@CategoryID int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT [Category].[ProductCategoryID], [Category].[Name]
		FROM [Production].[ProductCategory] AS [Category] 
		WHERE [Category].[ProductCategoryID] = @CategoryID;

	SELECT [Subcategory].[ProductSubcategoryID], [Subcategory].[Name], [Subcategory].[ProductCategoryID]
		FROM [Production].[ProductSubcategory] As [Subcategory]
	    WHERE [Subcategory].[ProductCategoryID] = @CategoryID;
END
GO

-- Create assembly.
CREATE ASSEMBLY [Dixin.Sql] 
FROM N'D:\OneDrive\Works\Drafts\CodeSnippets\Dixin.Sql\bin\Debug\Dixin.Sql.dll';
GO

-- Create aggregate from assembly.
CREATE AGGREGATE [Concat] (@value nvarchar(4000)) RETURNS nvarchar(max)
EXTERNAL NAME [Dixin.Sql].[Dixin.Sql.Concat];
GO

CREATE AGGREGATE [ConcatWith] (@value nvarchar(4000), @separator nvarchar(40)) RETURNS nvarchar(max)
EXTERNAL NAME [Dixin.Sql].[Dixin.Sql.ConcatWith];
GO

-- Call aggregate.
SELECT [Subcategory].[ProductCategoryID], COUNT([Subcategory].[Name]), [dbo].[Concat]([Subcategory].[Name])
FROM [Production].[ProductSubcategory] AS [Subcategory]
WHERE [Subcategory].[ProductCategoryID] < -1
GROUP BY [Subcategory].[ProductCategoryID];
GO

SELECT [dbo].[Concat](Name) FROM Production.ProductCategory
WHERE Production.ProductCategory.ProductCategoryID < -1;
GO

SELECT [Subcategory].[ProductCategoryID], COUNT([Subcategory].[Name]), [dbo].[Concat]([Subcategory].[Name])
FROM [Production].[ProductSubcategory] AS [Subcategory]
GROUP BY [Subcategory].[ProductCategoryID];
GO

SELECT [dbo].[Concat](Name) FROM Production.ProductCategory;
GO

SELECT [Subcategory].[ProductCategoryID], COUNT([Subcategory].[Name]), [dbo].[ConcatWith]([Subcategory].[Name], N' | ')
FROM [Production].[ProductSubcategory] AS [Subcategory]
WHERE [Subcategory].[ProductCategoryID] < -1
GROUP BY [Subcategory].[ProductCategoryID];
GO

SELECT [dbo].[ConcatWith](Name, N' | ') FROM Production.ProductCategory
WHERE Production.ProductCategory.ProductCategoryID < -1;
GO

SELECT [Subcategory].[ProductCategoryID], COUNT([Subcategory].[Name]), [dbo].[ConcatWith]([Subcategory].[Name], N' | ')
FROM [Production].[ProductSubcategory] AS [Subcategory]
GROUP BY [Subcategory].[ProductCategoryID];
GO

SELECT [dbo].[ConcatWith](Name, N' | ') FROM Production.ProductCategory;
GO