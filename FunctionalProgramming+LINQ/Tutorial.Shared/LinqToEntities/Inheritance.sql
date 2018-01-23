CREATE TABLE [Production].[TransactionHistory](
	[TransactionID] int IDENTITY(100000,1) NOT NULL
		CONSTRAINT [PK_TransactionHistory_TransactionID] PRIMARY KEY CLUSTERED,

	[ProductID] int NOT NULL
        CONSTRAINT [FK_TransactionHistory_Product_ProductID] FOREIGN KEY
        REFERENCES [Production].[Product] ([ProductID]),

	[TransactionDate] datetime NOT NULL,

	[TransactionType] nchar(1) NOT NULL
		CONSTRAINT [CK_Product_Style] 
        CHECK (UPPER([TransactionType]) = N'P' OR UPPER([TransactionType]) = N'S' OR UPPER([TransactionType]) = N'W'),

	[Quantity] int NOT NULL,

	[ActualCost] money NOT NULL

	/* Other columns. */);
GO
