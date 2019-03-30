CREATE VIEW [HumanResources].[vEmployee] 
AS 
SELECT 
    e.[BusinessEntityID],
    p.[FirstName],
    p.[LastName],
    e.[JobTitle]  
    -- Other columns.
FROM [HumanResources].[Employee] e
	INNER JOIN [Person].[Person] p
	ON p.[BusinessEntityID] = e.[BusinessEntityID]
    /* Other tables. */;
GO
