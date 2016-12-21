-- Create stored procedure.
CREATE PROCEDURE [dbo].[uspGetCategoryAndSubCategory]
    @CategoryID int
AS
BEGIN
    SELECT [Category].[ProductCategoryID], [Category].[Name]
        FROM [Production].[ProductCategory] AS [Category] 
        WHERE [Category].[ProductCategoryID] = @CategoryID;

    SELECT [Subcategory].[ProductSubcategoryID], [Subcategory].[Name], [Subcategory].[ProductCategoryID]
        FROM [Production].[ProductSubcategory] As [Subcategory]
        WHERE [Subcategory].[ProductCategoryID] = @CategoryID;
END;
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

SELECT [Subcategory].[ProductCategoryID], COUNT([Subcategory].[Name]), [dbo].[ConcatWith]([Subcategory].[Name], N' | ')
FROM [Production].[ProductSubcategory] AS [Subcategory]
WHERE [Subcategory].[ProductCategoryID] < -1
GROUP BY [Subcategory].[ProductCategoryID];
GO

SELECT [dbo].[ConcatWith](Name, N' | ') FROM Production.ProductCategory
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
GROUP BY [Subcategory].[ProductCategoryID];
GO

SELECT [dbo].[ConcatWith](Name, N' | ') FROM Production.ProductCategory;
GO

-- Create table-valued type.
CREATE TYPE [Category] AS TABLE (
    Id int,
    MinListPrice money);
GO

-- Create stored procedure with table-valued parameter.
CREATE PROCEDURE [dbo].[uspGetProducts]
    @Category [dbo].[Category] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Product].[ProductID], [Product].[Name], [Product].[ListPrice], [Product].[ProductSubcategoryID]
    FROM [Production].[Product] AS [Product]
    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Subcategory]
    ON [Product].ProductSubcategoryID = [Subcategory].[ProductSubcategoryID]
    LEFT OUTER JOIN @Category AS [Category]
    ON [Subcategory].[ProductCategoryID] = [Category].[Id]
    WHERE [Product].[ListPrice] >= [Category].[MinListPrice];
END;
GO

-- Call stored procedure with table-valued parameter.
DECLARE @Category AS [dbo].[Category];

INSERT INTO @Category ([Id], [MinListPrice])
    SELECT [ProductCategoryID], 300.00
    FROM [Production].[ProductCategory]

EXEC [dbo].[uspGetProducts] @Category;
GO

-- Create table-valued function with table-valued parameter.
CREATE FUNCTION [dbo].[ufnGetProducts]
(
    @Category [dbo].[Category] READONLY
)
RETURNS @Products TABLE 
(
    [ProductID] int NOT NULL, 
    [Name] nvarchar(50) NOT NULL, 
    [ListPrice] money NOT NULL, 
    [ProductSubcategoryID] int NULL
)
AS
BEGIN
    INSERT INTO @Products
        SELECT [Product].[ProductID], [Product].[Name], [Product].[ListPrice], [Product].[ProductSubcategoryID]
        FROM [Production].[Product] AS [Product]
        LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Subcategory]
        ON [Product].ProductSubcategoryID = [Subcategory].[ProductSubcategoryID]
        LEFT OUTER JOIN @Category AS [Category]
        ON [Subcategory].[ProductCategoryID] = [Category].[Id]
        WHERE [Product].[ListPrice] >= [Category].[MinListPrice];
    RETURN;
END;
GO

-- Call table-valued function with table-valued parameter.
DECLARE @Category AS [dbo].[Category];

INSERT INTO @Category ([Id], [MinListPrice])
    SELECT [ProductCategoryID], 300.00
    FROM [Production].[ProductCategory];

SELECT * FROM [dbo].[ufnGetProducts](@Category);
GO

-- Create scalar-valued function with table-valued parameter.
CREATE FUNCTION [dbo].[ufnGetProductCount]
(
    @Category [dbo].[Category] READONLY
)
RETURNS int
AS
BEGIN
    DECLARE @ProductCount int;

    SELECT @ProductCount = COUNT(*)
    FROM [Production].[Product] AS [Product]
    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Subcategory]
    ON [Product].ProductSubcategoryID = [Subcategory].[ProductSubcategoryID]
    LEFT OUTER JOIN @Category AS [Category]
    ON [Subcategory].[ProductCategoryID] = [Category].[Id]
    WHERE [Product].[ListPrice] >= [Category].[MinListPrice];

    RETURN @ProductCount;
END;
GO

-- Call scalar-valued function with table-valued parameter.
DECLARE @Category AS [dbo].[Category];

INSERT INTO @Category ([Id], [MinListPrice])
    SELECT [ProductCategoryID], 300.00
    FROM [Production].[ProductCategory];

SELECT [dbo].[ufnGetProductCount](@Category);
GO

-- Add rowversion column.
ALTER TABLE [Production].[Product] ADD [RowVersion] rowversion NOT NULL;
GO

-- Create trigger.
CREATE TRIGGER [Production].[uProductPhoto] ON [Production].[ProductPhoto]
AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE([ModifiedDate]) 
        RETURN;

    UPDATE [Production].[ProductPhoto]
    SET [ModifiedDate] = GETDATE()
    FROM [Production].[ProductPhoto]
    INNER JOIN [inserted]
    ON [ProductPhoto].[ProductPhotoID] = [inserted].[ProductPhotoID]
END;
GO

-- Update logic file name.
ALTER DATABASE [D:\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF]
    MODIFY FILE (NAME = N'AdventureWorks2014_Data', NEWNAME = N'AdventureWorks_Data');
GO

ALTER DATABASE [D:\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF]
    MODIFY FILE (NAME = N'AdventureWorks2014_Log', NEWNAME = N'AdventureWorks_Log');
GO

-- Shrink database and log.
DBCC SHRINKDATABASE ([D:\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF], 0, NOTRUNCATE)
GO

DECLARE @UsedSpaceInMB int;
SET @UsedSpaceInMB = (CAST(FILEPROPERTY(N'AdventureWorks_Data', 'SpaceUsed') AS int) / 128);
DBCC SHRINKFILE (AdventureWorks_Data, @UsedSpaceInMB);
GO

DBCC SHRINKFILE (AdventureWorks_Log, EMPTYFILE);
GO