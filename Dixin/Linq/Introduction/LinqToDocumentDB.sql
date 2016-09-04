SELECT
   CAST(BusinessEntityID AS varchar) AS [Id],
   Name AS [Name],
   AddressType AS [Address.AddressType],
   AddressLine1 AS [Address.AddressLine1],
   City AS [Address.Location.City],
   StateProvinceName AS [Address.Location.StateProvinceName],
   PostalCode AS [Address.PostalCode],
   CountryRegionName AS [Address.CountryRegionName]
FROM 
   Sales.vStoreWithAddresses
WHERE 
   AddressType = N'Main Office'