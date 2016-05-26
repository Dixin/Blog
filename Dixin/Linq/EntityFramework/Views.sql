CREATE VIEW [Production].[vProductAndDescriptionh] 
WITH SCHEMABINDING 
AS 
SELECT 
    [product].[ProductID],
    [product].[Name],
    [model].[Name] AS [ProductModel],
    [culture].[CultureID],
    [description].[Description] 
FROM [Production].[Product] [product]
    INNER JOIN [Production].[ProductModel] [model]
    ON [product].[ProductModelID] = model.[ProductModelID] 
    INNER JOIN [Production].[ProductModelProductDescriptionCulture] [culture]
    ON [model].[ProductModelID] = [culture].[ProductModelID] 
    INNER JOIN [Production].[ProductDescription] [description]
    ON [culture].[ProductDescriptionID] = [description].[ProductDescriptionID];
GO
