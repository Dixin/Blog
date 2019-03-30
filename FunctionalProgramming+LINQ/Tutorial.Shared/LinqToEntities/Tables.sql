CREATE SCHEMA [Production];
GO

CREATE TYPE [dbo].[Name] FROM nvarchar(50) NULL;
GO

CREATE TABLE [Production].[ProductCategory](
    [ProductCategoryID] int IDENTITY(1, 1) NOT NULL
        CONSTRAINT [PK_ProductCategory_ProductCategoryID] PRIMARY KEY CLUSTERED,

    [Name] [dbo].[Name] NOT NULL, -- nvarchar(50).

    [rowguid] uniqueidentifier ROWGUIDCOL NOT NULL -- Ignored in mapping.
        CONSTRAINT [DF_ProductCategory_rowguid] DEFAULT (NEWID()),
    
    [ModifiedDate] datetime NOT NULL -- Ignored in mapping.
        CONSTRAINT [DF_ProductCategory_ModifiedDate] DEFAULT (GETDATE()));
GO

CREATE TABLE [Production].[ProductSubcategory](
    [ProductSubcategoryID] int IDENTITY(1, 1) NOT NULL
        CONSTRAINT [PK_ProductSubcategory_ProductSubcategoryID] PRIMARY KEY CLUSTERED,

    [Name] [dbo].[Name] NOT NULL, -- nvarchar(50).

    [ProductCategoryID] int NOT NULL
        CONSTRAINT [FK_ProductSubcategory_ProductCategory_ProductCategoryID] FOREIGN KEY
        REFERENCES [Production].[ProductCategory] ([ProductCategoryID]),

    /* Other columns. */);
GO

CREATE TABLE [Production].[Product](
    [ProductID] int IDENTITY(1, 1) NOT NULL
        CONSTRAINT [PK_Product_ProductID] PRIMARY KEY CLUSTERED,

    [Name] [dbo].[Name] NOT NULL, -- nvarchar(50).

    [ListPrice] money NOT NULL,

    [ProductSubcategoryID] int NULL
        CONSTRAINT [FK_Product_ProductSubcategory_ProductSubcategoryID] FOREIGN KEY
        REFERENCES [Production].[ProductSubcategory] ([ProductSubcategoryID])
    
    /* Other columns. */);
GO

ALTER TABLE [Production].[Product] ADD [RowVersion] rowversion NOT NULL;
GO

CREATE TABLE [Production].[ProductPhoto](
    [ProductPhotoID] int IDENTITY(1, 1) NOT NULL
        CONSTRAINT [PK_ProductPhoto_ProductPhotoID] PRIMARY KEY CLUSTERED,

    [LargePhotoFileName] nvarchar(50) NULL,
    
    [ModifiedDate] datetime NOT NULL 
        CONSTRAINT [DF_ProductPhoto_ModifiedDate] DEFAULT (GETDATE())

    /* Other columns. */);
GO

CREATE TABLE [Production].[ProductProductPhoto](
    [ProductID] int NOT NULL
        CONSTRAINT [FK_ProductProductPhoto_Product_ProductID] FOREIGN KEY
        REFERENCES [Production].[Product] ([ProductID]),

    [ProductPhotoID] int NOT NULL
        CONSTRAINT [FK_ProductProductPhoto_ProductPhoto_ProductPhotoID] FOREIGN KEY
        REFERENCES [Production].[ProductPhoto] ([ProductPhotoID]),

    CONSTRAINT [PK_ProductProductPhoto_ProductID_ProductPhotoID] PRIMARY KEY NONCLUSTERED ([ProductID], [ProductPhotoID])
    
    /* Other columns. */);
GO

CREATE TABLE [Person].[Person](
	[BusinessEntityID] int NOT NULL
		CONSTRAINT [PK_Person_BusinessEntityID] PRIMARY KEY CLUSTERED,

	[FirstName] [dbo].[Name] NOT NULL,

	[LastName] [dbo].[Name] NOT NULL

	/* Other columns. */);
GO

CREATE TABLE [HumanResources].[Employee](
	[BusinessEntityID] int NOT NULL
		CONSTRAINT [PK_Employee_BusinessEntityID] PRIMARY KEY CLUSTERED
		CONSTRAINT [FK_Employee_Person_BusinessEntityID] FOREIGN KEY
        REFERENCES [Person].[Person] ([BusinessEntityID]),
	
	[JobTitle] nvarchar(50) NOT NULL,

	[HireDate] date NOT NULL

	/* Other columns. */);
GO
