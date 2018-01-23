USE [D:\onedrive\works\drafts\codesnippets\data\adventureworks_data.mdf];

DECLARE @EmployeeCount int;
SET @EmployeeCount = 40;

DECLARE @CustomerCount int;
SET @CustomerCount = 80;

DECLARE @ProductCount int;
SET @ProductCount = 50;

-- Delete employees.

DELETE FROM HumanResources.EmployeeDepartmentHistory
WHERE BusinessEntityID NOT IN (
        SELECT TOP (@EmployeeCount)
            BusinessEntityID
        FROM HumanResources.Employee
        ORDER BY BusinessEntityID);

DELETE FROM HumanResources.EmployeepayHistory
WHERE BusinessEntityID NOT IN (
        SELECT TOP (@EmployeeCount)
            BusinessEntityID
        FROM HumanResources.Employee
        ORDER BY BusinessEntityID);

DELETE Sales.SalesOrderHeader
    FROM Sales.SalesOrderHeader
    LEFT OUTER JOIN Sales.SalesPerson
        ON Sales.SalesOrderHeader.SalesPersonID = Sales.SalesPerson.BusinessEntityID
WHERE Sales.SalesPerson.BusinessEntityID NOT IN (
        SELECT TOP (@EmployeeCount)
            BusinessEntityID
        FROM HumanResources.Employee
        ORDER BY BusinessEntityID);

DELETE FROM Sales.SalesOrderDetail
WHERE SalesOrderDetail.SalesOrderID NOT IN (
        SELECT
            SalesOrderID
        FROM Sales.SalesOrderHeader);

DELETE Sales.SalesPersonQuotaHistory
    FROM Sales.SalesPersonQuotaHistory
    LEFT OUTER JOIN Sales.SalesPerson
        ON Sales.SalesPersonQuotaHistory.BusinessEntityID = Sales.SalesPerson.BusinessEntityID
WHERE Sales.SalesPerson.BusinessEntityID NOT IN (
        SELECT TOP (@EmployeeCount)
            BusinessEntityID
        FROM HumanResources.Employee
        ORDER BY BusinessEntityID);

DELETE Sales.SalesTerritoryHistory
    FROM Sales.SalesTerritoryHistory
WHERE Sales.SalesTerritoryHistory.BusinessEntityID NOT IN (
        SELECT TOP (@EmployeeCount)
            BusinessEntityID
        FROM HumanResources.Employee
        ORDER BY BusinessEntityID);

DELETE Sales.Customer
    FROM Sales.Customer
    LEFT OUTER JOIN Sales.Store
        ON Sales.Customer.StoreID = Sales.Store.BusinessEntityID
    LEFT OUTER JOIN Sales.SalesPerson
        ON Sales.Store.SalesPersonID = Sales.SalesPerson.BusinessEntityID
WHERE Sales.SalesPerson.BusinessEntityID NOT IN (
        SELECT TOP (@EmployeeCount)
            BusinessEntityID
        FROM HumanResources.Employee
        ORDER BY BusinessEntityID);

DELETE Sales.Store
    FROM Sales.Store
    LEFT OUTER JOIN Sales.SalesPerson
        ON Sales.Store.SalesPersonID = Sales.SalesPerson.BusinessEntityID
WHERE Sales.SalesPerson.BusinessEntityID NOT IN (
        SELECT TOP (@EmployeeCount)
            BusinessEntityID
        FROM HumanResources.Employee
        ORDER BY BusinessEntityID);

DELETE FROM Sales.SalesPerson
WHERE BusinessEntityID NOT IN (
        SELECT TOP (@EmployeeCount)
            BusinessEntityID
        FROM HumanResources.Employee
        ORDER BY BusinessEntityID);

DELETE HumanResources.JobCandidate
    FROM HumanResources.JobCandidate
    LEFT OUTER JOIN HumanResources.Employee
        ON HumanResources.JobCandidate.BusinessEntityID = HumanResources.Employee.BusinessEntityID
WHERE HumanResources.Employee.BusinessEntityID NOT IN (
        SELECT TOP (@EmployeeCount)
            BusinessEntityID
        FROM HumanResources.Employee
        ORDER BY BusinessEntityID);

DELETE Production.ProductDocument
    FROM Production.ProductDocument
    LEFT OUTER JOIN Production.Document
        ON Production.ProductDocument.DocumentNode = Production.Document.DocumentNode
    LEFT OUTER JOIN HumanResources.Employee
        ON Production.Document.Owner = HumanResources.Employee.BusinessEntityID
WHERE HumanResources.Employee.BusinessEntityID NOT IN (
        SELECT TOP (@EmployeeCount)
            BusinessEntityID
        FROM HumanResources.Employee
        ORDER BY BusinessEntityID);

DELETE Production.Document
    FROM Production.Document
    LEFT OUTER JOIN HumanResources.Employee
        ON Production.Document.Owner = HumanResources.Employee.BusinessEntityID
WHERE HumanResources.Employee.BusinessEntityID NOT IN (
        SELECT TOP (@EmployeeCount)
            BusinessEntityID
        FROM HumanResources.Employee
        ORDER BY BusinessEntityID);

DELETE Purchasing.PurchaseOrderDetail
    FROM Purchasing.PurchaseOrderDetail AS D
    LEFT OUTER JOIN Purchasing.PurchaseOrderHeader AS H
        ON D.PurchaseOrderID = H.PurchaseOrderID
WHERE H.Employeeid NOT IN (
        SELECT TOP (@EmployeeCount)
            BusinessEntityID
        FROM HumanResources.Employee
        ORDER BY BusinessEntityID);

DELETE Purchasing.PurchaseOrderHeader
    FROM Purchasing.PurchaseOrderHeader
    LEFT OUTER JOIN HumanResources.Employee
        ON Purchasing.PurchaseOrderHeader.Employeeid = HumanResources.Employee.BusinessEntityID
WHERE HumanResources.Employee.BusinessEntityID NOT IN (
        SELECT TOP (@EmployeeCount)
            BusinessEntityID
        FROM HumanResources.Employee
        ORDER BY BusinessEntityID);

ALTER TABLE HumanResources.Employee DISABLE TRIGGER Demployee;

DELETE FROM HumanResources.Employee
WHERE BusinessEntityID NOT IN (
        SELECT TOP (@EmployeeCount)
            BusinessEntityID
        FROM HumanResources.Employee
        ORDER BY BusinessEntityID);

ALTER TABLE HumanResources.Employee ENABLE TRIGGER Demployee;

-- Delete customers.

DELETE FROM Sales.SalesOrderHeader
WHERE Customerid NOT IN (
        SELECT TOP (@CustomerCount)
            Customerid
        FROM Sales.Customer);

DELETE FROM Sales.Customer
WHERE Customerid NOT IN (
        SELECT TOP (@CustomerCount)
            Customerid
        FROM Sales.Customer);
DELETE Sales.PersonCreditcard
    FROM Sales.PersonCreditcard
WHERE BusinessEntityID NOT IN (
        SELECT
            BusinessEntityID
        FROM HumanResources.Employee
        UNION ALL
        (SELECT
            PersonID
        FROM Sales.Customer));

--

DELETE FROM Person.Emailaddress
WHERE BusinessEntityID NOT IN (
        SELECT
            BusinessEntityID
        FROM HumanResources.Employee
        UNION ALL
        (SELECT
            PersonID
        FROM Sales.Customer));

DELETE Person.Password
    FROM Person.Password
WHERE BusinessEntityID NOT IN (
        SELECT
            BusinessEntityID
        FROM HumanResources.Employee
        UNION ALL
        (SELECT
            PersonID
        FROM Sales.Customer));

DELETE Person.PersonPhone
WHERE BusinessEntityID NOT IN (
        SELECT
            BusinessEntityID
        FROM HumanResources.Employee
        UNION ALL
        (SELECT
            PersonID
        FROM Sales.Customer));

DELETE Person.BusinessEntityContact
    FROM Person.Personphone
WHERE PersonID NOT IN (
        SELECT
            BusinessEntityID
        FROM HumanResources.Employee
        UNION ALL
        (SELECT
            PersonID
        FROM Sales.Customer));

-- Delete persons.

DELETE FROM Person.Person
WHERE BusinessEntityID NOT IN (
        SELECT
            BusinessEntityID
        FROM HumanResources.Employee
        UNION ALL
        (SELECT
            PersonID
        FROM Sales.Customer));

-- Delete products.

DELETE Production.ProductInventory
WHERE ProductID NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);

DELETE Production.ProductProductPhoto
WHERE ProductID NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);

DELETE Production.ProductPhoto
WHERE Productphotoid NOT IN (
        SELECT
            Productphotoid
        FROM Production.ProductProductPhoto);

DELETE FROM Purchasing.ProductVendor
WHERE ProductID NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);

DELETE FROM Production.TransactionHistory
WHERE ProductID NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);

DELETE FROM Production.TransactionHistoryArchive
WHERE ProductID NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);

DELETE FROM Production.BillOfMaterials
WHERE Productassemblyid NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);


DELETE FROM Production.BillOfMaterials
WHERE Componentid NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);

DELETE Production.WorkOrderRouting
    FROM Production.WorkOrderRouting
    LEFT OUTER JOIN Production.Workorder
        ON Production.WorkOrderRouting.WorkOrderID = Production.Workorder.WorkOrderID
WHERE Production.Workorder.ProductID NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);

DELETE Production.WorkOrder
WHERE ProductID NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);

DELETE Sales.SalesOrderDetail
    FROM Sales.SalesOrderDetail
WHERE ProductID NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);

DELETE Sales.SpecialOfferProduct
WHERE ProductID NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);

DELETE FROM Production.ProductCostHistory
WHERE ProductID NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);

DELETE FROM Production.ProductListPriceHistory
WHERE ProductID NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);

DELETE Production.ProductReview
WHERE ProductID NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);

DELETE Sales.ShoppingCartItem
WHERE ProductID NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);

DELETE Production.Product
WHERE ProductID NOT IN (
        SELECT TOP (@ProductCount)
            ProductID
        FROM Production.Product
        ORDER BY ProductID DESC);

DECLARE @Max int
SELECT @Max = MAX(ProductCategoryID) FROM Production.ProductCategory
IF @max IS NUll
  SET @max = 0
DBCC CHECKIDENT (N'Production.ProductCategory', RESEED, @max)

SELECT @Max = MAX(ProductSubcategoryID) FROM Production.ProductSubcategory
IF @max IS NUll
  SET @max = 0
DBCC CHECKIDENT (N'Production.ProductSubcategory', RESEED, @max)

SELECT @Max = MAX(ProductID) FROM Production.Product
IF @max IS NUll
  SET @max = 0
DBCC CHECKIDENT (N'Production.Product', RESEED, @max)

DELETE Production.ProductSubcategory
WHERE ProductSubcategoryID NOT IN (SELECT ProductSubcategoryID FROM Production.Product)
