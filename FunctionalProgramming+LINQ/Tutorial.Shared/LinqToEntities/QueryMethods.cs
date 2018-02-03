namespace Tutorial.LinqToEntities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static partial class QueryMethods
    {
        #region Filtering

        internal static void Where(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> source = adventureWorks.ProductCategories;
            IQueryable<ProductCategory> categories = source
                .Where(category => category.ProductCategoryID > 0); // Define query.
            categories.WriteLines(category => category.Name); // Execute query.
            // SELECT [category].[ProductCategoryID], [category].[Name]
            // FROM [Production].[ProductCategory] AS [category]
            // WHERE [category].[ProductCategoryID] > 0
        }

        internal static void WhereWithOr(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> source = adventureWorks.ProductCategories;
            IQueryable<ProductCategory> categories = source
                .Where(category =>
                category.ProductCategoryID <= 1 || category.ProductCategoryID >= 4); // Define query.
            categories.WriteLines(category => category.Name); // Execute query.
            // SELECT [category].[ProductCategoryID], [category].[Name]
            // FROM [Production].[ProductCategory] AS [category]
            // WHERE ([category].[ProductCategoryID] <= 1) OR ([category].[ProductCategoryID] >= 4)
        }

        internal static void WhereWithAnd(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> source = adventureWorks.ProductCategories;
            IQueryable<ProductCategory> categories = source.Where(category =>
                category.ProductCategoryID > 0 && category.ProductCategoryID < 5); // Define query.
            categories.WriteLines(category => category.Name); // Execute query.
            // SELECT [category].[ProductCategoryID], [category].[Name]
            // FROM [Production].[ProductCategory] AS [category]
            // WHERE ([category].[ProductCategoryID] > 0) AND ([category].[ProductCategoryID] < 5)
        }

        internal static void WhereAndWhere(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> source = adventureWorks.ProductCategories;
            IQueryable<ProductCategory> categories = source
                .Where(category => category.ProductCategoryID > 0)
                .Where(category => category.ProductCategoryID < 5); // Define query.
            categories.WriteLines(category => category.Name); // Execute query.
            // SELECT [category].[ProductCategoryID], [category].[Name]
            // FROM [Production].[ProductCategory] AS [category]
            // WHERE ([category].[ProductCategoryID] > 0) AND ([category].[ProductCategoryID] < 5)
        }

        internal static void WhereWithIs(AdventureWorks adventureWorks)
        {
            IQueryable<TransactionHistory> source = adventureWorks.Transactions;
            IQueryable<TransactionHistory> transactions = source.Where(transaction => transaction is SalesTransactionHistory); // Define query.
            transactions.WriteLines(transaction => $"{transaction.GetType().Name} {transaction.TransactionDate} {transaction.ActualCost}"); // Execute query.
            // SELECT [transaction].[TransactionID], [transaction].[ActualCost], [transaction].[ProductID], [transaction].[Quantity], [transaction].[TransactionDate], [transaction].[TransactionType]
            // FROM [Production].[TransactionHistory] AS [transaction]
            // WHERE [transaction].[TransactionType] IN (N'W', N'S', N'P') AND ([transaction].[TransactionType] = N'S')
        }

        internal static void OfTypeEntity(AdventureWorks adventureWorks)
        {
            IQueryable<TransactionHistory> source = adventureWorks.Transactions;
            IQueryable<WorkTransactionHistory> transactions = source.OfType<WorkTransactionHistory>(); // Define query.
            transactions.WriteLines(transaction => $"{transaction.GetType().Name} {transaction.TransactionDate} {transaction.ActualCost}"); // Execute query.
            // SELECT [t].[TransactionID], [t].[ActualCost], [t].[ProductID], [t].[Quantity], [t].[TransactionDate], [t].[TransactionType]
            // FROM [Production].[TransactionHistory] AS [t]
            // WHERE [t].[TransactionType] = N'W'
        }

        internal static void OfTypePrimitive(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            IQueryable<int> products = source.Select(product => product.ProductSubcategoryID).OfType<int>(); // Define query.
            products.ToArray().Length.WriteLine(); // Execute query.
#if EF
            // NotSupportedException: 'System.Int32' is not a valid metadata type for type filtering operations. Type filtering is only valid on entity types and complex types.
#else
            // SELECT [p].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [p]
#endif
        }

        #endregion

        #region Mapping

        internal static void Select(AdventureWorks adventureWorks)
        {
            IQueryable<Person> source = adventureWorks.People;
            IQueryable<string> names = source.Select(person =>
                person.FirstName + " " + person.LastName); // Define query.
            names.WriteLines(); // Execute query.
            // SELECT ([person].[FirstName] + N' ') + [person].[LastName]
            // FROM [Person].[Person] AS [person]
        }

        internal static void SelectWithStringConcat(AdventureWorks adventureWorks)
        {
            IQueryable<Person> source = adventureWorks.People;
            IQueryable<string> categories = source.Select(category =>
                string.Concat(category.FirstName, " ", category.LastName)); // Define query.
            categories.WriteLines(); // Execute query.
            // SELECT [category].[FirstName], [category].[LastName]
            // FROM [Person].[Person] AS [category]
        }

        internal static void SelectEntity(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            IQueryable<Product> products = source
                .Where(product => product.ListPrice > 1_000)
                .Select(product => new Product()
                {
                    ProductID = product.ProductID,
                    Name = product.Name
                }); // Define query.
            products.WriteLines(product => $"{product.ProductID}: {product.Name}"); // Execute query.
#if EF
            // NotSupportedException: The entity or complex type 'Product' cannot be constructed in a LINQ to Entities query.
#else
            // SELECT [product].[ProductID], [product].[Name]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] > 1000.0
#endif
        }

        internal static void SelectAnonymousType(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var products = source.Select(product =>
                new { Name = product.Name, IsExpensive = product.ListPrice > 1_000 }); // Define query.
            products.WriteLines(); // Execute query.
            // SELECT [product].[Name], CASE
            //    WHEN [product].[ListPrice] > 1000.0
            //    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
            // END
            // FROM [Production].[Product] AS [product]
        }

        #endregion

        #region Generation

        internal static void DefaultIfEmptyEntity(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> source = adventureWorks.ProductCategories;
            IQueryable<ProductCategory> categories = source
                .Where(category => category.ProductCategoryID < 0)
                .DefaultIfEmpty(); // Define query.
            categories.ForEach( // Execute query.
                category => (category == null).WriteLine()); // True
#if EF
            // SELECT 
            //    [Project1].[ProductCategoryID] AS [ProductCategoryID], 
            //    [Project1].[Name] AS [Name]
            //    FROM   ( SELECT 1 AS X ) AS [SingleRowTable1]
            //    LEFT OUTER JOIN  (SELECT 
            //        [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            //        [Extent1].[Name] AS [Name]
            //        FROM [Production].[ProductCategory] AS [Extent1]
            //        WHERE [Extent1].[ProductCategoryID] < 0 ) AS [Project1] ON 1 = 1
#else
            // SELECT [t].[ProductCategoryID], [t].[Name]
            // FROM (
            //    SELECT NULL AS [empty]
            // ) AS [empty]
            // LEFT JOIN (
            //    SELECT [category].[ProductCategoryID], [category].[Name]
            //    FROM [Production].[ProductCategory] AS [category]
            //    WHERE [category].[ProductCategoryID] < 0
            // ) AS [t] ON 1 = 1
#endif
        }

        internal static void DefaultIfEmptyPrimitive(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> source = adventureWorks.ProductCategories;
            IQueryable<int> categories = source
                .Where(category => category.ProductCategoryID < 0)
                .Select(category => category.ProductCategoryID)
                .DefaultIfEmpty(); // Define query.
            categories.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    CASE WHEN ([Project1].[C1] IS NULL) THEN 0 ELSE [Project1].[ProductCategoryID] END AS [C1]
            //    FROM   ( SELECT 1 AS X ) AS [SingleRowTable1]
            //    LEFT OUTER JOIN  (SELECT 
            //        [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            //        cast(1 as tinyint) AS [C1]
            //        FROM [Production].[ProductCategory] AS [Extent1]
            //        WHERE [Extent1].[ProductCategoryID] < 0 ) AS [Project1] ON 1 = 1
#else
            // SELECT [t].[ProductCategoryID]
            // FROM (
            //    SELECT NULL AS [empty]
            // ) AS [empty]
            // LEFT JOIN (
            //    SELECT [category].[ProductCategoryID]
            //    FROM [Production].[ProductCategory] AS [category]
            //    WHERE [category].[ProductCategoryID] < 0
            // ) AS [t] ON 1 = 1
#endif
        }

        internal static void DefaultIfEmptyWithDefaultEntity(AdventureWorks adventureWorks)
        {
            ProductCategory @default = new ProductCategory() { Name = nameof(ProductCategory) };
            IQueryable<ProductCategory> source = adventureWorks.ProductCategories;
            IQueryable<ProductCategory> categories = source
                .Where(category => category.ProductCategoryID < 0)
                .DefaultIfEmpty(@default); ; // Define query.
            categories.WriteLines( // Execute query.
                category => category?.Name); // ProductCategory
#if EF
            // NotSupportedException: Unable to create a constant value of type 'ProductCategory'. Only primitive types or enumeration types are supported in this context.
#else
            // SELECT [category].[ProductCategoryID], [category].[Name]
            // FROM [Production].[ProductCategory] AS [category]
            // WHERE [category].[ProductCategoryID] < 0
#endif
        }

        internal static void DefaultIfEmptyWithDefaultPrimitive(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> source = adventureWorks.ProductCategories;
            IQueryable<int> categories = source
                .Where(category => category.ProductCategoryID < 0)
                .Select(category => category.ProductCategoryID)
                .DefaultIfEmpty(-1); // Define query.
            categories.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    CASE WHEN ([Project1].[C1] IS NULL) THEN -1 ELSE [Project1].[ProductCategoryID] END AS [C1]
            //    FROM   ( SELECT 1 AS X ) AS [SingleRowTable1]
            //    LEFT OUTER JOIN  (SELECT 
            //        [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            //        cast(1 as tinyint) AS [C1]
            //        FROM [Production].[ProductCategory] AS [Extent1]
            //        WHERE [Extent1].[ProductCategoryID] < 0 ) AS [Project1] ON 1 = 1
#else
            // SELECT [category].[ProductCategoryID]
            // FROM [Production].[ProductCategory] AS [category]
            // WHERE [category].[ProductCategoryID] < 0
#endif
        }

        #endregion

        #region Grouping

        internal static void GroupBy(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> source = adventureWorks.ProductSubcategories;
            IQueryable<IGrouping<int, string>> groups = source.GroupBy(
                keySelector: subcategory => subcategory.ProductCategoryID,
                elementSelector: subcategory => subcategory.Name); // Define query.
            groups.WriteLines(group => $"{group.Key}: {string.Join(", ", group)}"); // Execute query.
#if EF
            // SELECT 
            //    [Project2].[ProductCategoryID] AS [ProductCategoryID], 
            //    [Project2].[C1] AS [C1], 
            //    [Project2].[Name] AS [Name]
            //    FROM ( SELECT 
            //        [Distinct1].[ProductCategoryID] AS [ProductCategoryID], 
            //        [Extent2].[Name] AS [Name], 
            //        CASE WHEN ([Extent2].[ProductCategoryID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C1]
            //        FROM   (SELECT DISTINCT 
            //            [Extent1].[ProductCategoryID] AS [ProductCategoryID]
            //            FROM [Production].[ProductSubcategory] AS [Extent1] ) AS [Distinct1]
            //        LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Distinct1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
            //    )  AS [Project2]
            //    ORDER BY [Project2].[ProductCategoryID] ASC, [Project2].[C1] ASC
#else
            // SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
            // FROM [Production].[ProductSubcategory] AS [subcategory]
            // ORDER BY [subcategory].[ProductCategoryID]
#endif
        }

        internal static void GroupByWithResultSelector(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> source = adventureWorks.ProductSubcategories;
            var groups = source.GroupBy(
                keySelector: subcategory => subcategory.ProductCategoryID,
                elementSelector: subcategory => subcategory.Name,
                resultSelector: (key, group) => new { CategoryID = key, SubcategoryCount = group.Count() }); // Define query.
            groups.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [GroupBy1].[K1] AS [ProductCategoryID], 
            //    [GroupBy1].[A1] AS [C1]
            //    FROM ( SELECT 
            //        [Extent1].[ProductCategoryID] AS [K1], 
            //        COUNT(1) AS [A1]
            //        FROM [Production].[ProductSubcategory] AS [Extent1]
            //        GROUP BY [Extent1].[ProductCategoryID]
            //    )  AS [GroupBy1]
#else
            // SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
            // FROM [Production].[ProductSubcategory] AS [subcategory]
            // ORDER BY [subcategory].[ProductCategoryID]
#endif
        }

        internal static void GroupByAndSelect(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> source = adventureWorks.ProductSubcategories;
            var groups = source
                .GroupBy(
                    keySelector: subcategory => subcategory.ProductCategoryID,
                    elementSelector: subcategory => subcategory.Name)
                .Select(group => new { CategoryID = group.Key, SubcategoryCount = group.Count() }); // Define query.
            groups.WriteLines(); // Execute query.
        }

        internal static void GroupByAndSelectMany(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> source = adventureWorks.ProductSubcategories;
            IQueryable<ProductSubcategory> distinct = source
                .GroupBy(keySelector: subcategory => subcategory.ProductCategoryID)
                .SelectMany(group => group); // Define query.
            distinct.WriteLines(subcategory => subcategory.Name); // Execute query.
#if EF
            // SELECT 
            //    [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID], 
            //    [Extent2].[Name] AS [Name], 
            //    [Extent2].[ProductCategoryID] AS [ProductCategoryID]
            //    FROM   (SELECT DISTINCT 
            //        [Extent1].[ProductCategoryID] AS [ProductCategoryID]
            //        FROM [Production].[ProductSubcategory] AS [Extent1] ) AS [Distinct1]
            //    INNER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Distinct1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
#else
            // SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
            // FROM [Production].[ProductSubcategory] AS [subcategory]
            // ORDER BY [subcategory].[ProductCategoryID]
#endif
        }

        internal static void GroupByMultipleKeys(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var groups = source.GroupBy(
                keySelector: product => new { ProductSubcategoryID = product.ProductSubcategoryID, ListPrice = product.ListPrice },
                resultSelector: (key, group) => new
                {
                    ProductSubcategoryID = key.ProductSubcategoryID,
                    ListPrice = key.ListPrice,
                    Count = group.Count()
                }); // Define query.
            groups.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    1 AS [C1], 
            //    [GroupBy1].[K2] AS [ProductSubcategoryID], 
            //    [GroupBy1].[K1] AS [ListPrice], 
            //    [GroupBy1].[A1] AS [C2]
            //    FROM ( SELECT 
            //        [Extent1].[ListPrice] AS [K1], 
            //        [Extent1].[ProductSubcategoryID] AS [K2], 
            //        COUNT(1) AS [A1]
            //        FROM [Production].[Product] AS [Extent1]
            //        GROUP BY [Extent1].[ListPrice], [Extent1].[ProductSubcategoryID]
            //    )  AS [GroupBy1]
#else
            // SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [product]
            // ORDER BY [product].[ProductSubcategoryID], [product].[ListPrice]
#endif
        }

        #endregion

        #region Join

        internal static void InnerJoinWithJoin(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categorySubcategories = outer.Join(
                inner: inner,
                outerKeySelector: category => category.ProductCategoryID,
                innerKeySelector: subcategory => subcategory.ProductCategoryID,
                resultSelector: (category, subcategory) =>
                    new { Category = category.Name, Subcategory = subcategory.Name }); // Define query.
            // var categorySubcategories =
            //    from category in outer
            //    join subcategory in inner
            //    on category.ProductCategoryID equals subcategory.ProductCategoryID
            //    select new { Category = category.Name, Subcategory = subcategory.Name };
            categorySubcategories.WriteLines(); // Execute query.
            // SELECT [category].[Name], [subcategory].[Name]
            // FROM [Production].[ProductCategory] AS [category]
            // INNER JOIN [Production].[ProductSubcategory] AS [subcategory] ON [category].[ProductCategoryID] = [subcategory].[ProductCategoryID]
        }

        internal static void InnerJoinWithSelect(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categorySubcategories = outer
                .Select(category => new
                {
                    Category = category,
                    Subcategories = inner
                        .Where(subcategory => category.ProductCategoryID == subcategory.ProductCategoryID)
                    // LEFT OUTER JOIN if DefaultIfEmpty is called.
                })
                .SelectMany(
                    collectionSelector: category => category.Subcategories,
                    resultSelector: (category, subcategory) =>
                        new { Category = category.Category.Name, Subcategory = subcategory.Name }); // Define query.
            // var categorySubcategories =
            //    from category in outer
            //    select new
            //    {
            //        Category = category,
            //        Subcategories = from subcategory in inner
            //                        where category.ProductCategoryID == subcategory.ProductCategoryID
            //                        select subcategory
            //    } into category
            //    from subcategory in category.Subcategories
            //    select new { Category = category.Category.Name, Subcategory = subcategory.Name };
            categorySubcategories.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            //    [Extent1].[Name] AS [Name], 
            //    [Extent2].[Name] AS [Name1]
            //    FROM  [Production].[ProductCategory] AS [Extent1]
            //    INNER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
#else
            // SELECT [category].[Name], [subcategory].[Name]
            // FROM [Production].[ProductCategory] AS [category]
            // CROSS JOIN [Production].[ProductSubcategory] AS [subcategory]
            // WHERE [category].[ProductCategoryID] = [subcategory].[ProductCategoryID]
#endif
        }

        internal static void InnerJoinWithSelectMany(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categorySubcategories = outer
                .SelectMany(
                    collectionSelector: category => inner
                        .Where(subcategory => category.ProductCategoryID == subcategory.ProductCategoryID),
                    // LEFT OUTER JOIN if DefaultIfEmpty is called.
                    resultSelector: (category, subcategory) =>
                        new { Category = category.Name, Subcategory = subcategory.Name }); // Define query.
            // var categorySubcategories =
            //   from category in outer
            //   from subcategory in (from subcategory in inner
            //                        where category.ProductCategoryID == subcategory.ProductCategoryID
            //                        select subcategory)
            //   select new { Category = category.Name, Subcategory = subcategory.Name };
            // Or equivalently:
            // var categorySubcategories =
            //    from category in outer
            //    from subcategory in inner
            //    where category.ProductCategoryID == subcategory.ProductCategoryID
            //    select new { Category = category.Name, Subcategory = subcategory.Name };
            categorySubcategories.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            //    [Extent1].[Name] AS [Name], 
            //    [Extent2].[Name] AS [Name1]
            //    FROM  [Production].[ProductCategory] AS [Extent1]
            //    INNER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
#else
            // SELECT [category].[Name], [subcategory].[Name]
            // FROM [Production].[ProductCategory] AS [category]
            // CROSS JOIN [Production].[ProductSubcategory] AS [subcategory]
            // WHERE [category].[ProductCategoryID] = [subcategory].[ProductCategoryID]
#endif
        }

        internal static void InnerJoinWithSelectAndRelationship(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            var categorySubcategories = outer
                .Select(category => new { Category = category, Subcategories = category.ProductSubcategories })
                .SelectMany(
                    collectionSelector: category => category.Subcategories,
                    // LEFT OUTER JOIN if DefaultIfEmpty is missing.
                    resultSelector: (category, subcategory) =>
                        new { Category = category.Category.Name, Subcategory = subcategory.Name }); // Define query.
            // var categorySubcategories =
            //    from category in outer
            //    select new { Category = category, Subcategories = category.ProductSubcategories } into category
            //    from subcategory in category.Subcategories
            //    select new { Category = category.Category.Name, Subcategory = subcategory.Name };
            categorySubcategories.WriteLines(); // Execute query.
            // SELECT [category].[Name], [category].[ProductCategoryID], [category.ProductSubcategories].[Name]
            // FROM [Production].[ProductCategory] AS [category]
            // INNER JOIN [Production].[ProductSubcategory] AS [category.ProductSubcategories] ON [category].[ProductCategoryID] = [category.ProductSubcategories].[ProductCategoryID]
        }

        internal static void InnerJoinWithSelectManyAndRelationship(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            var categorySubcategories = outer.SelectMany(
                collectionSelector: category => category.ProductSubcategories,
                // LEFT OUTER JOIN if DefaultIfEmpty is missing.
                resultSelector: (category, subcategory) =>
                    new { Category = category.Name, Subcategory = subcategory.Name }); // Define query.
            // var categorySubcategories =
            //    from category in outer
            //    from subcategory in category.ProductSubcategories
            //    select new { Category = category.Name, Subcategory = subcategory.Name };
            categorySubcategories.WriteLines(); // Execute query.
            // SELECT [category].[Name], [category.ProductSubcategories].[Name]
            // FROM [Production].[ProductCategory] AS [category]
            // INNER JOIN [Production].[ProductSubcategory] AS [category.ProductSubcategories] ON [category].[ProductCategoryID] = [category.ProductSubcategories].[ProductCategoryID]
        }

        internal static void InnerJoinWithMultipleKeys(AdventureWorks adventureWorks)
        {
            IQueryable<Product> outer = adventureWorks.Products;
            IQueryable<TransactionHistory> inner = adventureWorks.Transactions;
            var transactions = outer.Join(
                inner: inner,
                outerKeySelector: product =>
                    new { ProductID = product.ProductID, UnitPrice = product.ListPrice },
                innerKeySelector: transaction =>
                    new { ProductID = transaction.ProductID, UnitPrice = transaction.ActualCost / transaction.Quantity },
                resultSelector: (product, transaction) =>
                    new { Name = product.Name, Quantity = transaction.Quantity }); // Define query.
            // var transactions =
            //    from product in adventureWorks.Products
            //    join transaction in adventureWorks.Transactions
            //    on new { ProductID = product.ProductID, UnitPrice = product.ListPrice }
            //        equals new { ProductID = transaction.ProductID, UnitPrice = transaction.ActualCost / transaction.Quantity }
            //    select new { Name = product.Name, Quantity = transaction.Quantity };
            transactions.WriteLines(); // Execute query.
            // SELECT [product].[Name], [transaction].[Quantity]
            // FROM [Production].[Product] AS [product]
            // INNER JOIN [Production].[TransactionHistory] AS [transaction] ON ([product].[ProductID] = [transaction].[ProductID]) AND ([product].[ListPrice] = ([transaction].[ActualCost] / [transaction].[Quantity]))
        }

        internal static void MultipleInnerJoinsWithRelationship(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var productPhotos = source.SelectMany(
                collectionSelector: product => product.ProductProductPhotos,
                resultSelector: (product, productProductPhoto) => new
                {
                    Product = product.Name,
                    Photo = productProductPhoto.ProductPhoto.LargePhotoFileName
                }); // Define query.
            // var productPhotos =
            //    from product in source
            //    from productProductPhoto in product.ProductProductPhotos
            //    select new { Product = product.Name, Photo = productProductPhoto.ProductPhoto.LargePhotoFileName };
            productPhotos.WriteLines(); // Execute query.
            // SELECT [product].[Name], [product.ProductProductPhotos.ProductPhoto].[LargePhotoFileName]
            // FROM [Production].[Product] AS [product]
            // INNER JOIN [Production].[ProductProductPhoto] AS [product.ProductProductPhotos] ON [product].[ProductID] = [product.ProductProductPhotos].[ProductID]
            // INNER JOIN [Production].[ProductPhoto] AS [product.ProductProductPhotos.ProductPhoto] ON [product.ProductProductPhotos].[ProductPhotoID] = [product.ProductProductPhotos.ProductPhoto].[ProductPhotoID]
        }

        internal static void InnerJoinWithGroupJoinAndSelectMany(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categorySubcategories = outer
                .GroupJoin(
                    inner: inner,
                    outerKeySelector: category => category.ProductCategoryID,
                    innerKeySelector: subcategory => subcategory.ProductCategoryID,
                    resultSelector: (category, subcategories) =>
                        new { Category = category, Subcategories = subcategories })
                .SelectMany(
                    collectionSelector: category => category.Subcategories,
                    // LEFT OUTER JOIN if DefaultIfEmpty is called.
                    resultSelector: (category, subcategory) =>
                        new { Category = category.Category.Name, Subcategory = subcategory.Name }); // Define query.
            // var categorySubcategories =
            //    from category in outer
            //    join subcategory in inner
            //    on category.ProductCategoryID equals subcategory.ProductCategoryID into subcategories
            //    from subcategory in subcategories
            //    select new { Category = category.Name, Subcategory = subcategory.Name };
            categorySubcategories.WriteLines(); // Execute query.
            // SELECT [category].[Name], [subcategory].[Name]
            // FROM [Production].[ProductCategory] AS [category]
            // INNER JOIN [Production].[ProductSubcategory] AS [subcategory] ON [category].[ProductCategoryID] = [subcategory].[ProductCategoryID]
        }

        internal static void LeftOuterJoinWithGroupJoin(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categorySubcategories = outer
                .GroupJoin(
                    inner: inner,
                    outerKeySelector: category => category.ProductCategoryID,
                    innerKeySelector: subcategory => subcategory.ProductCategoryID,
                    resultSelector: (category, subcategories) =>
                        new { Category = category, Subcategories = subcategories }); // Define query.
            // var categorySubcategories =
            //    from category in outer
            //    join subcategory in inner
            //    on category.ProductCategoryID equals subcategory.ProductCategoryID into subcategories
            //    select new { Category = category, Subcategories = subcategories };
            categorySubcategories.WriteLines(categorySubcategory =>
                $@"{categorySubcategory.Category.Name}: {string.Join(
                    ", ", categorySubcategory.Subcategories.Select(subcategory => subcategory.Name))}"); // Execute query.
            // SELECT [category].[ProductCategoryID], [category].[Name], [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
            // FROM [Production].[ProductCategory] AS [category]
            // LEFT JOIN [Production].[ProductSubcategory] AS [subcategory] ON [category].[ProductCategoryID] = [subcategory].[ProductCategoryID]
            // ORDER BY [category].[ProductCategoryID]
        }

        internal static void LeftOuterJoinWithGroupJoinAndSelectMany(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categorySubcategories = outer
                .GroupJoin(
                    inner: inner,
                    outerKeySelector: category => category.ProductCategoryID,
                    innerKeySelector: subcategory => subcategory.ProductCategoryID,
                    resultSelector: (category, subcategories) =>
                        new { Category = category, Subcategories = subcategories }) // Define query.
                .SelectMany(
                    collectionSelector: category => category.Subcategories
                        .DefaultIfEmpty(), // INNER JOIN if DefaultIfEmpty is missing.
                    resultSelector: (category, subcategory) =>
                        new { Category = category.Category, Subcategory = subcategory }); // Define query.
            // var categorySubcategories =
            //    from category in outer
            //    join subcategory in inner
            //    on category.ProductCategoryID equals subcategory.ProductCategoryID into subcategories
            //    from subcategory in subcategories.DefaultIfEmpty()
            //    select new { Category = category.Name, Subcategory = subcategory.Name };
            categorySubcategories.WriteLines(categorySubcategory =>
                $"{categorySubcategory.Category.Name} {categorySubcategory.Subcategory?.Name}"); // Execute query.
            // SELECT [category].[ProductCategoryID], [category].[Name], [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
            // FROM [Production].[ProductCategory] AS [category]
            // LEFT JOIN [Production].[ProductSubcategory] AS [subcategory] ON [category].[ProductCategoryID] = [subcategory].[ProductCategoryID]
            // ORDER BY [category].[ProductCategoryID]
        }

        internal static void LeftOuterJoinWithSelect(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categorySubcategories = outer
                .Select(category => new
                {
                    Category = category,
                    Subcategories = inner
                        .Where(subcategory => category.ProductCategoryID == subcategory.ProductCategoryID)
                })
                .SelectMany(
                    collectionSelector: category => category.Subcategories
                        .DefaultIfEmpty(), // INNER JOIN if DefaultIfEmpty is missing.
                    resultSelector: (category, subcategory) =>
                        new { Category = category.Category.Name, Subcategory = subcategory.Name }); // Define query.
            // var categorySubcategories =
            //    from category in outer
            //    select new
            //    {
            //        Category = category,
            //        Subcategories = from subcategory in inner
            //                        where subcategory.ProductCategoryID == category.ProductCategoryID
            //                        select subcategory
            //    } into category
            //    from subcategory in category.Subcategories.DefaultIfEmpty()
            //    select new { Category = category.Category.Name, Subcategory = subcategory.Name };
            categorySubcategories.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            //    [Extent1].[Name] AS [Name], 
            //    [Extent2].[Name] AS [Name1]
            //    FROM  [Production].[ProductCategory] AS [Extent1]
            //    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
#else
            // SELECT [category].[Name], [t1].[Name]
            // FROM [Production].[ProductCategory] AS [category]
            // CROSS APPLY (
            //    SELECT [t0].*
            //    FROM (
            //        SELECT NULL AS [empty]
            //    ) AS [empty0]
            //    LEFT JOIN (
            //        SELECT [subcategory0].*
            //        FROM [Production].[ProductSubcategory] AS [subcategory0]
            //        WHERE [category].[ProductCategoryID] = [subcategory0].[ProductCategoryID]
            //    ) AS [t0] ON 1 = 1
            // ) AS [t1]
#endif
        }

        internal static void LeftOuterJoinWithSelectMany(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categorySubcategories = outer
                .SelectMany(
                    collectionSelector: category => inner
                        .Where(subcategory => category.ProductCategoryID == subcategory.ProductCategoryID)
                        .DefaultIfEmpty(), // INNER JOIN if DefaultIfEmpty is missing.
                    resultSelector: (category, subcategory) =>
                        new { Category = category.Name, Subcategory = subcategory.Name }); // Define query.
            // var categorySubcategories =
            //    from category in outer
            //    from subcategory in (from subcategory in inner
            //                         where category.ProductCategoryID == subcategory.ProductCategoryID
            //                         select subcategory).DefaultIfEmpty()
            //    select new { Category = category.Name, Subcategory = subcategory.Name };
            categorySubcategories.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            //    [Extent1].[Name] AS [Name], 
            //    [Extent2].[Name] AS [Name1]
            //    FROM  [Production].[ProductCategory] AS [Extent1]
            //    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
#else
            // SELECT [category].[Name], [t1].[Name]
            // FROM [Production].[ProductCategory] AS [category]
            // CROSS APPLY (
            //    SELECT [t0].*
            //    FROM (
            //        SELECT NULL AS [empty]
            //    ) AS [empty0]
            //    LEFT JOIN (
            //        SELECT [subcategory0].*
            //        FROM [Production].[ProductSubcategory] AS [subcategory0]
            //        WHERE [category].[ProductCategoryID] = [subcategory0].[ProductCategoryID]
            //    ) AS [t0] ON 1 = 1
            // ) AS [t1]
#endif
        }

        internal static void LeftOuterJoinWithSelectAndRelationship(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            var categorySubcategories = outer
                .Select(category => new { Category = category, Subcategories = category.ProductSubcategories })
                .SelectMany(
                    collectionSelector: category => category.Subcategories
                        .DefaultIfEmpty(), // INNER JOIN if DefaultIfEmpty is missing.
                    resultSelector: (category, subcategory) =>
                        new { Category = category.Category.Name, Subcategory = subcategory.Name }); // Define query.
            // var categorySubcategories =
            //    from category in outer
            //    select new { Category = category, Subcategories = category.ProductSubcategories } into category
            //    from subcategory in category.Subcategories.DefaultIfEmpty()
            //    select new { Category = category.Category.Name, Subcategory = subcategory.Name };
            categorySubcategories.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            //    [Extent1].[Name] AS [Name], 
            //    [Extent2].[Name] AS [Name1]
            //    FROM  [Production].[ProductCategory] AS [Extent1]
            //    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
#else
            // SELECT [category].[Name] AS [Category], [category.ProductSubcategories].[Name] AS [Subcategory]
            // FROM [Production].[ProductCategory] AS [category]
            // LEFT JOIN [Production].[ProductSubcategory] AS [category.ProductSubcategories] ON [category].[ProductCategoryID] = [category.ProductSubcategories].[ProductCategoryID]
#endif
        }

        internal static void LeftOuterJoinWithSelectManyAndRelationship(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            var categorySubcategories = outer.SelectMany(
                collectionSelector: category => category.ProductSubcategories
                    .DefaultIfEmpty(), // INNER JOIN if DefaultIfEmpty is missing.
                resultSelector: (category, subcategory) =>
                    new { Category = category.Name, Subcategory = subcategory.Name }); // Define query.
            // var categorySubcategories =
            //    from category in outer
            //    from subcategory in category.ProductSubcategories.DefaultIfEmpty()
            //    select new { Category = category.Name, Subcategory = subcategory.Name };
            categorySubcategories.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            //    [Extent1].[Name] AS [Name], 
            //    [Extent2].[Name] AS [Name1]
            //    FROM  [Production].[ProductCategory] AS [Extent1]
            //    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Extent2] ON [Extent1].[ProductCategoryID] = [Extent2].[ProductCategoryID]
#else
            // SELECT [category].[Name] AS [Category], [category.ProductSubcategories].[Name] AS [Subcategory]
            // FROM [Production].[ProductCategory] AS [category]
            // LEFT JOIN [Production].[ProductSubcategory] AS [category.ProductSubcategories] ON [category].[ProductCategoryID] = [category.ProductSubcategories].[ProductCategoryID]
#endif
        }

        internal static void CrossJoinWithSelectMany(AdventureWorks adventureWorks)
        {
            IQueryable<Product> outer = adventureWorks.Products.Where(product => product.ListPrice > 2000);
            IQueryable<Product> inner = adventureWorks.Products.Where(product => product.ListPrice < 100);
            var bundles = outer.SelectMany(
                collectionSelector: expensiveProduct => inner,
                resultSelector: (expensiveProduct, cheapProduct) =>
                    new { Expensive = expensiveProduct.Name, Cheap = cheapProduct.Name }); // Define query.
            // var bundles =
            //    from outerProduct in outer
            //    from innerProduct in inner
            //    select new { Expensive = outerProduct.Name, Cheap = innerProduct.Name };
            bundles.WriteLines(); // Execute query.
            // SELECT [product].[Name], [product0].[Name]
            // FROM [Production].[Product] AS [product]
            // CROSS JOIN [Production].[Product] AS [product0]
            // WHERE ([product].[ListPrice] > 2000.0) AND ([product0].[ListPrice] < 100.0)
        }

        internal static void CrossJoinWithJoin(AdventureWorks adventureWorks)
        {
            IQueryable<Product> outer = adventureWorks.Products.Where(product => product.ListPrice > 2000);
            IQueryable<Product> inner = adventureWorks.Products.Where(product => product.ListPrice < 100);
            var bundles = outer.Join(
                inner: inner,
                outerKeySelector: product => 1,
                innerKeySelector: product => 1,
                resultSelector: (outerProduct, innerProduct) =>
                    new { Expensive = outerProduct.Name, Cheap = innerProduct.Name }); // Define query.
            // var bundles =
            //    from outerProduct in outer
            //    join innerProduct in inner
            //    on 1 equals 1
            //    select new { Expensive = outerProduct.Name, Cheap = innerProduct.Name };
            bundles.WriteLines(); // Execute query.
            // SELECT [product].[Name], [t].[Name]
            // FROM [Production].[Product] AS [product]
            // INNER JOIN (
            //    SELECT [product1].*
            //    FROM [Production].[Product] AS [product1]
            //    WHERE [product1].[ListPrice] < 100.0
            // ) AS [t] ON 1 = 1
            // WHERE [product].[ListPrice] > 2000.0
        }

        internal static void SelfJoin(AdventureWorks adventureWorks)
        {
            IQueryable<Product> outer = adventureWorks.Products;
            IQueryable<Product> inner = adventureWorks.Products;
            var products = outer.GroupJoin(
                inner,
                product => product.ListPrice,
                product => product.ListPrice,
                (product, samePriceProducts) => new
                {
                    Name = product.Name,
                    ListPrice = product.ListPrice,
                    SamePriceProducts = samePriceProducts
                        .Where(samePriceProduct => samePriceProduct.ProductID != product.ProductID)
                        .Select(samePriceProduct => samePriceProduct.Name)
                }); // Define query.
            // var products =
            //    from outerProduct in outer
            //    join innerProduct in inner
            //    on outerProduct.ListPrice equals innerProduct.ListPrice into samePriceProducts
            //    select new
            //    {
            //        Name = outerProduct.Name,
            //        ListPrice = outerProduct.ListPrice,
            //        SamePriceProducts = from samePriceProduct in samePriceProducts
            //                            where samePriceProduct.ProductID != outerProduct.ProductID
            //                            select samePriceProduct.Name
            //    };
            products.WriteLines(product =>
                $"{product.Name} ({product.ListPrice}): {string.Join(", ", product.SamePriceProducts)}"); // Execute query.
            // SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID], [product0].[ProductID], [product0].[ListPrice], [product0].[Name], [product0].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [product]
            // LEFT JOIN [Production].[Product] AS [product0] ON [product].[ListPrice] = [product0].[ListPrice]
            // ORDER BY [product].[ListPrice]
        }

        #endregion

        #region Apply

        internal static void CrossApplyWithGroupByAndTake(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> source = adventureWorks.ProductSubcategories;
            var categories = source
                .GroupBy(subcategory => subcategory.ProductCategoryID)
                .SelectMany(
                    group => group.Take(1),
                    (group, subcategory) =>
                        new { ProductCategoryID = group.Key, FirstSubcategory = subcategory }); // Define query.
            categories.WriteLines(category =>
                $"{category.ProductCategoryID}: {category.FirstSubcategory?.Name}"); // Execute query.
        }

        internal static void CrossApplyWithGroupJoinAndTake(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categories = outer
                .GroupJoin(
                    inner,
                    category => category.ProductCategoryID,
                    subcategory => subcategory.ProductCategoryID,
                    (category, subcategories) => new { Category = category, Subcategories = subcategories })
                .SelectMany(
                    category => category.Subcategories.Take(1),
                    (category, subcategory) =>
                        new { Category = category.Category, FirstSubcategory = subcategory }); // Define query.
            categories.WriteLines(category =>
                $"{category.Category.Name}: {category.FirstSubcategory?.Name}"); // Execute query.
        }

        internal static void CrossApplyWithRelationshipAndTake(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> source = adventureWorks.ProductCategories;
            var categories = source
                .Select(category => new { Category = category, Subcategories = category.ProductSubcategories })
                .SelectMany(
                    collectionSelector: category => category.Subcategories.Take(1),
                    resultSelector: (category, subcategory) =>
                        new { Category = category.Category, FirstSubcategory = subcategory }); // Define query.
            categories.WriteLines(category =>
                $"{category.Category.Name}: {category.FirstSubcategory?.Name}"); // Execute query.
        }

        internal static void OuterApplyWithGroupByAndFirstOrDefault(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> source = adventureWorks.ProductSubcategories;
            var categories = source.GroupBy(
                subcategory => subcategory.ProductCategoryID,
                (key, group) =>
                    new { ProductCategoryID = key, FirstSubcategory = group.FirstOrDefault() }); // Define query.
            categories.WriteLines(category =>
                $"{category.ProductCategoryID}: {category.FirstSubcategory?.Name}"); // Execute query.
        }

        internal static void OuterApplyWithGroupJoinAndFirstOrDefault(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categories = outer.GroupJoin(
                inner,
                category => category.ProductCategoryID,
                subcategory => subcategory.ProductCategoryID,
                (category, subcategories) =>
                    new { Category = category, FirstSubcategory = subcategories.FirstOrDefault() }); // Define query.
            categories.WriteLines(category =>
                $"{category.Category.Name}: {category.FirstSubcategory?.Name}"); // Execute query.
        }

        internal static void OuterApplyWithRelationshipAndFirstOrDefault(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> source = adventureWorks.ProductCategories;
            var categories = source.Select(category => new
            {
                Category = category,
                FirstSubcategory = category.ProductSubcategories.FirstOrDefault()
            }); // Define query.
            categories.WriteLines(category =>
                $"{category.Category.Name}: {category.FirstSubcategory?.Name}"); // Execute query.
        }

        #endregion

        #region Concatenation

        internal static void ConcatPrimitive(AdventureWorks adventureWorks)
        {
            IQueryable<string> first = adventureWorks.Products
                .Where(product => product.ListPrice < 100)
                .Select(product => product.Name);
            IQueryable<string> second = adventureWorks.Products
                .Where(product => product.ListPrice > 2000)
                .Select(product => product.Name);
            IQueryable<string> concat = first.Concat(second); // Define query.
            concat.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [UnionAll1].[Name] AS [C1]
            //    FROM  (SELECT 
            //        [Extent1].[Name] AS [Name]
            //        FROM [Production].[Product] AS [Extent1]
            //        WHERE [Extent1].[ListPrice] < cast(100 as decimal(18))
            //    UNION ALL
            //        SELECT 
            //        [Extent2].[Name] AS [Name]
            //        FROM [Production].[Product] AS [Extent2]
            //        WHERE [Extent2].[ListPrice] > cast(2000 as decimal(18))) AS [UnionAll1]
#else
            // SELECT [product].[Name]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] < 100.0

            // SELECT [product0].[Name]
            // FROM [Production].[Product] AS [product0]
            // WHERE [product0].[ListPrice] > 2000.0
#endif
        }

        internal static void ConcatEntity(AdventureWorks adventureWorks)
        {
            IQueryable<Product> first = adventureWorks.Products.Where(product => product.ListPrice < 100);
            IQueryable<Product> second = adventureWorks.Products.Where(product => product.ListPrice > 2000);
            IQueryable<string> concat = first
                .Concat(second)
                .Select(product => product.Name); // Define query.
            concat.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [UnionAll1].[Name] AS [C1]
            //    FROM  (SELECT 
            //        [Extent1].[Name] AS [Name]
            //        FROM [Production].[Product] AS [Extent1]
            //        WHERE [Extent1].[ListPrice] < cast(100 as decimal(18))
            //    UNION ALL
            //        SELECT 
            //        [Extent2].[Name] AS [Name]
            //        FROM [Production].[Product] AS [Extent2]
            //        WHERE [Extent2].[ListPrice] > cast(2000 as decimal(18))) AS [UnionAll1]
#else
            // ArgumentException: Expression of type 'System.Collections.Generic.IEnumerable`1[Product]' cannot be used for parameter of type 'System.Collections.Generic.IEnumerable`1[Microsoft.EntityFrameworkCore.Storage.ValueBuffer]' of method 'System.Collections.Generic.IEnumerable`1[Microsoft.EntityFrameworkCore.Storage.ValueBuffer] Concat[ValueBuffer](System.Collections.Generic.IEnumerable`1[Microsoft.EntityFrameworkCore.Storage.ValueBuffer], System.Collections.Generic.IEnumerable`1[Microsoft.EntityFrameworkCore.Storage.ValueBuffer])' Parameter name: arg1
#endif
        }

        #endregion

        #region Set

        internal static void DistinctEntity(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> source = adventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> distinct = source
                .Select(subcategory => subcategory.ProductCategory)
                .Distinct(); // Define query.
            distinct.WriteLines(category => $"{category.ProductCategoryID}: {category.Name}"); // Execute query.
            // SELECT DISTINCT [subcategory.ProductCategory].[ProductCategoryID], [subcategory.ProductCategory].[Name]
            // FROM [Production].[ProductSubcategory] AS [subcategory]
            // INNER JOIN [Production].[ProductCategory] AS [subcategory.ProductCategory] ON [subcategory].[ProductCategoryID] = [subcategory.ProductCategory].[ProductCategoryID]
        }

        internal static void DistinctPrimitive(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> source = adventureWorks.ProductSubcategories;
            IQueryable<int> distinct = source
                .Select(subcategory => subcategory.ProductCategoryID)
                .Distinct(); // Define query.
            distinct.WriteLines(); // Execute query.
            // SELECT DISTINCT [subcategory].[ProductCategoryID]
            // FROM [Production].[ProductSubcategory] AS [subcategory]
        }

        internal static void DistinctMultipleKeys(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var distinct = source
                .Select(product =>
                    new { ProductSubcategoryID = product.ProductSubcategoryID, ListPrice = product.ListPrice })
                .Distinct(); // Define query.
            distinct.WriteLines(); // Execute query.
            // SELECT DISTINCT [product].[ProductSubcategoryID], [product].[ListPrice]
            // FROM [Production].[Product] AS [product]
        }

        internal static void DistinctEntityWithGroupBy(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> source = adventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> distinct = source.GroupBy(
                keySelector: subcategory => subcategory.ProductCategory,
                resultSelector: (key, group) => key); // Define query.
            distinct.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Distinct1].[ProductCategoryID] AS [ProductCategoryID]
            //    FROM ( SELECT DISTINCT 
            //        [Extent1].[ProductCategoryID] AS [ProductCategoryID]
            //        FROM [Production].[ProductSubcategory] AS [Extent1]
            //    )  AS [Distinct1]
#else
            // SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
            // FROM [Production].[ProductSubcategory] AS [subcategory]
            // ORDER BY [subcategory].[ProductCategoryID]
#endif
        }

        internal static void DistinctWithGroupBy(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> source = adventureWorks.ProductSubcategories;
            IQueryable<int> distinct = source.GroupBy(
                keySelector: subcategory => subcategory.ProductCategoryID,
                resultSelector: (key, group) => key); // Define query.
            distinct.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Distinct1].[ProductCategoryID] AS [ProductCategoryID]
            //    FROM ( SELECT DISTINCT 
            //        [Extent1].[ProductCategoryID] AS [ProductCategoryID]
            //        FROM [Production].[ProductSubcategory] AS [Extent1]
            //    )  AS [Distinct1]
#else
            // SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
            // FROM [Production].[ProductSubcategory] AS [subcategory]
            // ORDER BY [subcategory].[ProductCategoryID]
#endif
        }

        internal static void DistinctMultipleKeysWithGroupBy(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> source = adventureWorks.ProductSubcategories;
            var distinct = source.GroupBy(
                keySelector: subcategory => new { ProductCategoryID = subcategory.ProductCategoryID, Name = subcategory.Name },
                resultSelector: (key, group) => key); // Define query.
            distinct.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Distinct1].[C1] AS [C1], 
            //    [Distinct1].[ProductCategoryID] AS [ProductCategoryID], 
            //    [Distinct1].[Name] AS [Name]
            //    FROM ( SELECT DISTINCT 
            //        [Extent1].[Name] AS [Name], 
            //        [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            //        1 AS [C1]
            //        FROM [Production].[ProductSubcategory] AS [Extent1]
            //    )  AS [Distinct1]
#else
            // SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
            // FROM [Production].[ProductSubcategory] AS [subcategory]
            // ORDER BY [subcategory].[ProductCategoryID], [subcategory].[Name]
#endif
        }

        internal static void DistinctWithGroupByAndFirstOrDefault(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            IQueryable<Product> distinct = source.GroupBy(
                keySelector: product => product.ListPrice,
                resultSelector: (key, group) => group.FirstOrDefault()); // Define query.
            distinct.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Limit1].[ProductID] AS [ProductID], 
            //    [Limit1].[Name] AS [Name], 
            //    [Limit1].[ListPrice] AS [ListPrice], 
            //    [Limit1].[ProductSubcategoryID] AS [ProductSubcategoryID]
            //    FROM   (SELECT DISTINCT 
            //        [Extent1].[ListPrice] AS [ListPrice]
            //        FROM [Production].[Product] AS [Extent1] ) AS [Distinct1]
            //    OUTER APPLY  (SELECT TOP (1) 
            //        [Extent2].[ProductID] AS [ProductID], 
            //        [Extent2].[Name] AS [Name], 
            //        [Extent2].[ListPrice] AS [ListPrice], 
            //        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID]
            //        FROM [Production].[Product] AS [Extent2]
            //        WHERE [Distinct1].[ListPrice] = [Extent2].[ListPrice] ) AS [Limit1]
#else
            // SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [product]
            // ORDER BY [product].[ListPrice]
#endif
        }

        internal static void UnionEntity(AdventureWorks adventureWorks)
        {
            IQueryable<Product> first = adventureWorks.Products
                .Where(product => product.ListPrice > 100);
            IQueryable<Product> second = adventureWorks.Products
                .Where(product => product.ProductSubcategoryID == 1);
            IQueryable<Product> union = first.Union(second); // Define query.
            union.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Distinct1].[C1] AS [C1], 
            //    [Distinct1].[C2] AS [C2], 
            //    [Distinct1].[C3] AS [C3], 
            //    [Distinct1].[C4] AS [C4], 
            //    [Distinct1].[C5] AS [C5]
            //    FROM ( SELECT DISTINCT 
            //        [UnionAll1].[ProductID] AS [C1], 
            //        [UnionAll1].[Name] AS [C2], 
            //        [UnionAll1].[ListPrice] AS [C3], 
            //        [UnionAll1].[ProductSubcategoryID] AS [C4]
            //        FROM  (SELECT 
            //            [Extent1].[ProductID] AS [ProductID], 
            //            [Extent1].[Name] AS [Name], 
            //            [Extent1].[ListPrice] AS [ListPrice], 
            //            [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID]
            //            FROM [Production].[Product] AS [Extent1]
            //            WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))
            //        UNION ALL
            //            SELECT 
            //            [Extent2].[ProductID] AS [ProductID], 
            //            [Extent2].[Name] AS [Name], 
            //            [Extent2].[ListPrice] AS [ListPrice], 
            //            [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID]
            //            FROM [Production].[Product] AS [Extent2]
            //            WHERE 1 = [Extent2].[ProductSubcategoryID]) AS [UnionAll1]
            //    )  AS [Distinct1]
#else
            // SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] > 100.0

            // SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [product]
            // [product0].[ProductSubcategoryID] = 1
#endif
        }

        internal static void UnionPrimitive(AdventureWorks adventureWorks)
        {
            var first = adventureWorks.Products
                .Where(product => product.ListPrice > 100)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice });
            var second = adventureWorks.Products
                .Where(product => product.ProductSubcategoryID == 1)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice });
            var union = first.Union(second); // Define query.
            union.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Distinct1].[C1] AS [C1], 
            //    [Distinct1].[C2] AS [C2], 
            //    [Distinct1].[C3] AS [C3]
            //    FROM ( SELECT DISTINCT 
            //        [UnionAll1].[C1] AS [C1], 
            //        [UnionAll1].[Name] AS [C2], 
            //        [UnionAll1].[ListPrice] AS [C3]
            //        FROM  (SELECT 
            //            1 AS [C1], 
            //            [Extent1].[Name] AS [Name], 
            //            [Extent1].[ListPrice] AS [ListPrice]
            //            FROM [Production].[Product] AS [Extent1]
            //            WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))
            //        UNION ALL
            //            SELECT 
            //            1 AS [C1], 
            //            [Extent2].[Name] AS [Name], 
            //            [Extent2].[ListPrice] AS [ListPrice]
            //            FROM [Production].[Product] AS [Extent2]
            //            WHERE 1 = [Extent2].[ProductSubcategoryID]) AS [UnionAll1]
            //    )  AS [Distinct1]
#else
            // SELECT [product].[Name], [product].[ListPrice]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] > 100.0

            // SELECT [product0].[Name], [product0].[ListPrice]
            // FROM [Production].[Product] AS [product0]
            // WHERE [product0].[ProductSubcategoryID] = 1
#endif
        }

        internal static void IntersectEntity(AdventureWorks adventureWorks)
        {
            IQueryable<Product> first = adventureWorks.Products
                .Where(product => product.ListPrice > 100);
            IQueryable<Product> second = adventureWorks.Products
                .Where(product => product.ListPrice < 2000);
            IQueryable<Product> intersect = first.Intersect(second); // Define query.
            intersect.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Intersect1].[ProductID] AS [C1], 
            //    [Intersect1].[Name] AS [C2], 
            //    [Intersect1].[ListPrice] AS [C3]
            //    FROM  (SELECT 
            //        [Extent1].[ProductID] AS [ProductID], 
            //        [Extent1].[Name] AS [Name], 
            //        [Extent1].[ListPrice] AS [ListPrice], 
            //        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID]
            //        FROM [Production].[Product] AS [Extent1]
            //        WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))
            //    INTERSECT
            //        SELECT 
            //        [Extent2].[ProductID] AS [ProductID], 
            //        [Extent2].[Name] AS [Name], 
            //        [Extent2].[ListPrice] AS [ListPrice], 
            //        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID]
            //        FROM [Production].[Product] AS [Extent2]
            //        WHERE [Extent2].[ListPrice] < cast(2000 as decimal(18))) AS [Intersect1]
#else
            // SELECT [product0].[ProductID], [product0].[ListPrice], [product0].[Name], [product0].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [product0]
            // WHERE [product0].[ListPrice] < 2000.0

            // SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] > 100.0
#endif
        }

        internal static void IntersectPrimitive(AdventureWorks adventureWorks)
        {
            var first = adventureWorks.Products
                .Where(product => product.ListPrice > 100)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice });
            var second = adventureWorks.Products
                .Where(product => product.ListPrice < 2000)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice });
            var intersect = first.Intersect(second); // Define query.
            intersect.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Intersect1].[C1] AS [C1], 
            //    [Intersect1].[Name] AS [C2], 
            //    [Intersect1].[ListPrice] AS [C3]
            //    FROM  (SELECT 
            //        1 AS [C1], 
            //        [Extent1].[Name] AS [Name], 
            //        [Extent1].[ListPrice] AS [ListPrice]
            //        FROM [Production].[Product] AS [Extent1]
            //        WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))
            //    INTERSECT
            //        SELECT 
            //        1 AS [C1], 
            //        [Extent2].[Name] AS [Name], 
            //        [Extent2].[ListPrice] AS [ListPrice]
            //        FROM [Production].[Product] AS [Extent2]
            //        WHERE [Extent2].[ListPrice] < cast(2000 as decimal(18))) AS [Intersect1]
#else
            // SELECT [product0].[Name], [product0].[ListPrice]
            // FROM [Production].[Product] AS [product0]
            // WHERE [product0].[ListPrice] < 2000.0

            // SELECT [product].[Name], [product].[ListPrice]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] > 100.0
#endif
        }

        internal static void ExceptEntity(AdventureWorks adventureWorks)
        {
            IQueryable<Product> first = adventureWorks.Products
                .Where(product => product.ListPrice > 100);
            IQueryable<Product> second = adventureWorks.Products
                .Where(product => product.ListPrice > 2000);
            IQueryable<Product> except = first.Except(second); // Define query.
            except.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Except1].[ProductID] AS [C1], 
            //    [Except1].[Name] AS [C2], 
            //    [Except1].[ListPrice] AS [C3]
            //    FROM  (SELECT 
            //        [Extent1].[ProductID] AS [ProductID], 
            //        [Extent1].[Name] AS [Name], 
            //        [Extent1].[ListPrice] AS [ListPrice], 
            //        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID]
            //        FROM [Production].[Product] AS [Extent1]
            //        WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))
            //    EXCEPT
            //        SELECT 
            //        [Extent2].[ProductID] AS [ProductID], 
            //        [Extent2].[Name] AS [Name], 
            //        [Extent2].[ListPrice] AS [ListPrice], 
            //        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID]
            //        FROM [Production].[Product] AS [Extent2]
            //        WHERE [Extent2].[ListPrice] > cast(2000 as decimal(18))) AS [Except1]
#else
            // SELECT [product0].[ProductID], [product0].[ListPrice], [product0].[Name], [product0].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [product0]
            // WHERE [product0].[ListPrice] > 2000.0

            // SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] > 100.0
#endif
        }

        internal static void ExceptPrimitive(AdventureWorks adventureWorks)
        {
            var first = adventureWorks.Products
                .Where(product => product.ListPrice > 100)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice });
            var second = adventureWorks.Products
                .Where(product => product.ListPrice > 2000)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice });
            var except = first.Except(second); // Define query.
            except.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Except1].[C1] AS [C1], 
            //    [Except1].[Name] AS [C2], 
            //    [Except1].[ListPrice] AS [C3]
            //    FROM  (SELECT 
            //        1 AS [C1], 
            //        [Extent1].[Name] AS [Name], 
            //        [Extent1].[ListPrice] AS [ListPrice]
            //        FROM [Production].[Product] AS [Extent1]
            //        WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))
            //    EXCEPT
            //        SELECT 
            //        1 AS [C1], 
            //        [Extent2].[Name] AS [Name], 
            //        [Extent2].[ListPrice] AS [ListPrice]
            //        FROM [Production].[Product] AS [Extent2]
            //        WHERE [Extent2].[ListPrice] > cast(2000 as decimal(18))) AS [Except1]
#else
            // SELECT [product0].[Name], [product0].[ListPrice]
            // FROM [Production].[Product] AS [product0]
            // WHERE [product0].[ListPrice] > 2000.0

            // SELECT [product].[Name], [product].[ListPrice]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] > 100.0
#endif
        }

        #endregion

        #region Convolution

        internal static void Zip(AdventureWorks adventureWorks)
        {
            IQueryable<Product> first = adventureWorks.Products.OrderBy(product => product.ListPrice);
            IQueryable<Product> second = adventureWorks.Products.OrderByDescending(product => product.ListPrice);
            IQueryable<string> listProces = first.Zip(second, (firstProduct, secondProduct) => firstProduct.ListPrice + " " + secondProduct.ListPrice);
            listProces.WriteLines(); // Execute query.
#if EF
            // NotSupportedException
#else
            // NotImplementedException
#endif
        }

        #endregion

        #region Partitioning

        internal static void Skip(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            IQueryable<string> names = source
                .Select(product => product.Name)
                .Skip(10); // Define query.
            names.WriteLines(); // Execute query.
#if EF
            // NotSupportedException: The method 'Skip' is only supported for sorted input in LINQ to Entities. The method 'OrderBy' must be called before the method 'Skip'.
#else
            // exec sp_executesql N'SELECT [product].[Name]
            // FROM [Production].[Product] AS [product]
            // ORDER BY (SELECT 1)
            // OFFSET @__p_0 ROWS',N'@__p_0 int',@__p_0=10
#endif
        }

        internal static void OrderByAndSkip(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            IQueryable<string> names = source
                .Select(product => product.Name)
                .OrderBy(product => 1)
                .Skip(10); // Define query.
            names.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Project1].[Name] AS [Name]
            //    FROM ( SELECT 
            //        1 AS [C1], 
            //        [Extent1].[Name] AS [Name]
            //        FROM [Production].[Product] AS [Extent1]
            //    )  AS [Project1]
            //    ORDER BY [Project1].[C1] ASC
            //    OFFSET 10 ROWS 
#else
            // exec sp_executesql N'SELECT [product].[Name]
            // FROM [Production].[Product] AS [product]
            // ORDER BY [product].[Name]
            // OFFSET @__p_0 ROWS',N'@__p_0 int',@__p_0=10
#endif
        }

        internal static void Take(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            IQueryable<string> products = source
                .Take(10)
                .Select(product => product.Name); // Define query.
            products.WriteLines(); // Execute query.
            // exec sp_executesql N'SELECT [t].[Name]
            // FROM (
            //    SELECT TOP(@__p_0) [p0].*
            //    FROM [Production].[Product] AS [p0]
            // ) AS [t]',N'@__p_0 int',@__p_0=10
        }

        internal static void OrderByAndSkipAndTake(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            IQueryable<string> products = source
                .OrderBy(product => product.Name)
                .Skip(20)
                .Take(10)
                .Select(product => product.Name); // Define query.
            products.WriteLines(); // Execute query.
            // exec sp_executesql N'SELECT [t].[Name]
            // FROM (
            //    SELECT [product0].*
            //    FROM [Production].[Product] AS [product0]
            //    ORDER BY [product0].[Name]
            //    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
            // ) AS [t]',N'@__p_0 int,@__p_1 int',@__p_0=20,@__p_1=10
        }

        internal static void TakeWhile(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            IQueryable<Product> products = source.TakeWhile(product => product.ListPrice < 1_000); // Define query.
            products.WriteLines(product => product.Name); // Execute query.
            // NotSupportedException: Could not parse expression 'value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Product]).SkipWhile(product => (product.ListPrice < 1000))': This overload of the method 'System.Linq.Queryable.SkipWhile' is currently not supported.
        }

        internal static void SkipWhile(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            IQueryable<Product> products = source.SkipWhile(product => product.ListPrice < 1_000); // Define query.
            products.WriteLines(product => product.Name); // Execute query.
            // NotSupportedException: Could not parse expression 'value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Product]).TakeWhile(product => (product.ListPrice < 1000))': This overload of the method 'System.Linq.Queryable.TakeWhile' is currently not supported.
        }

        #endregion

        #region Ordering

        internal static void OrderBy(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var products = source
                .OrderBy(product => product.ListPrice)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }); // Define query.
            products.WriteLines(); // Execute query.
            // SELECT [product].[Name], [product].[ListPrice]
            // FROM [Production].[Product] AS [product]
            // ORDER BY [product].[ListPrice]
        }

        internal static void OrderByDescending(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var products = source
                .OrderByDescending(product => product.ListPrice)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }); // Define query.
            products.WriteLines(); // Execute query.
            // SELECT [product].[Name], [product].[ListPrice]
            // FROM [Production].[Product] AS [product]
            // ORDER BY [product].[ListPrice] DESC
        }

        internal static void OrderByAndThenBy(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var products = source
                .OrderBy(product => product.ListPrice)
                .ThenBy(product => product.Name)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }); // Define query.
            products.WriteLines(); // Execute query.
            // SELECT [product].[Name], [product].[ListPrice]
            // FROM [Production].[Product] AS [product]
            // ORDER BY [product].[ListPrice], [product].[Name]
        }

        internal static void OrderByMultipleKeys(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var products = source
                .OrderBy(product => new { ListPrice = product.ListPrice, Name = product.Name })
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }); // Define query.
            products.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Project1].[C1] AS [C1], 
            //    [Project1].[Name] AS [Name], 
            //    [Project1].[ListPrice] AS [ListPrice]
            //    FROM ( SELECT 
            //        [Extent1].[Name] AS [Name], 
            //        [Extent1].[ListPrice] AS [ListPrice], 
            //        1 AS [C1]
            //        FROM [Production].[Product] AS [Extent1]
            //    )  AS [Project1]
            //    ORDER BY [Project1].[ListPrice] ASC, [Project1].[Name] ASC
#else
            // SELECT [product].[Name], [product].[ListPrice]
            // FROM [Production].[Product] AS [product]
            // ORDER BY (SELECT 1)
#endif
        }

        internal static void OrderByAndOrderBy(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var products = source
                .OrderBy(product => product.ListPrice)
                .OrderBy(product => product.ProductSubcategoryID)
                .Select(product => new
                {
                    Name = product.Name,
                    ListPrice = product.ListPrice,
                    Subcategory = product.ProductSubcategoryID
                }); // Define query.
            products.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    [Project1].[C1] AS [C1], 
            //    [Project1].[Name] AS [Name], 
            //    [Project1].[ListPrice] AS [ListPrice], 
            //    [Project1].[ProductSubcategoryID] AS [ProductSubcategoryID]
            //    FROM ( SELECT 
            //        [Extent1].[Name] AS [Name], 
            //        [Extent1].[ListPrice] AS [ListPrice], 
            //        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
            //        1 AS [C1]
            //        FROM [Production].[Product] AS [Extent1]
            //    )  AS [Project1]
            //    ORDER BY [Project1].[ProductSubcategoryID] ASC
#else
            // SELECT [product].[Name], [product].[ListPrice], [product].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [product]
            // ORDER BY [product].[ProductSubcategoryID], [product].[ListPrice]
#endif
        }

        internal static void Reverse(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var products = source
                .OrderBy(product => product.ListPrice)
                .Reverse()
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }); // Define query.
            products.WriteLines(); // Execute query.
#if EF
            // NotSupportedException: LINQ to Entities does not recognize the method 'System.Linq.IQueryable`1[Product] Reverse[Product](System.Linq.IQueryable`1[Product])' method, and this method cannot be translated into a store expression.
#else
            // NotImplementedException: Remotion.Linq.Clauses.ResultOperators.ReverseResultOperator
#endif
        }

        #endregion

        #region Conversion

        internal static void CastEntity(AdventureWorks adventureWorks)
        {
            IQueryable<TransactionHistory> source = adventureWorks.Transactions;
            IQueryable<TransactionHistory> transactions = source
                .Where(product => product.ActualCost > 500)
                .Cast<SalesTransactionHistory>(); // Define query.
            transactions.WriteLines(transaction =>
                $"{transaction.GetType().Name}: {transaction.TransactionDate}"); // Execute query.
#if EF
            // NotSupportedException: Unable to cast the type 'TransactionHistory' to type 'SalesTransactionHistory'. LINQ to Entities only supports casting EDM primitive or enumeration types.
#else
            // SELECT [product].[TransactionID], [product].[ActualCost], [product].[ProductID], [product].[Quantity], [product].[TransactionDate], [product].[TransactionType]
            // FROM [Production].[TransactionHistory] AS [product]
            // WHERE [product].[TransactionType] IN (N'W', N'S', N'P') AND ([product].[ActualCost] > 500.0)
#endif
        }

        internal static void CastPrimitive(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            IQueryable<string> listPrices = source
                .Select(product => product.ListPrice)
                .Cast<string>(); // Define query.
            listPrices.WriteLines(); // Execute query.
#if EF
            // SELECT 
            //    CAST( [Extent1].[ListPrice] AS nvarchar(max)) AS [C1]
            //    FROM [Production].[Product] AS [Extent1]
#else
            // InvalidOperationException: No coercion operator is defined between types 'System.Decimal' and 'System.String'.
#endif
        }

        internal static void AsEnumerableAsQueryable(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var remoteAndLocal = source // DbSet<T>.
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }) // Return EntityQueryable<T>.
                .AsEnumerable() // Do nothing. Directly return the EntityQueryable<T> source.
                .Where(product => product.ListPrice > 0) // Enumerable.Where. Return a generator wrapping the EntityQueryable<T> source.
                .AsQueryable() // Return an EnumerableQuery<T> instance wrapping the source generator.
                .OrderBy(product => product.Name); // Queryable.OrderBy. Return EnumerableQuery<T>.
            remoteAndLocal.WriteLines();
            // SELECT [product].[Name], [product].[ListPrice]
            // FROM [Production].[Product] AS [product]

            var remote = source // DbSet<T>.
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }) // Return EntityQueryable<T>.
                .AsEnumerable() // Do nothing. Directly return the EntityQueryable<T> source.
                .AsQueryable() // Do nothing. Directly return the EntityQueryable<T> source.
                .Where(product => product.ListPrice > 0) // Still LINQ to Entities. Return EntityQueryable<T>.
                .OrderBy(product => product.Name); // Still LINQ to Entities. Return EntityQueryable<T>.
            remote.WriteLines();
            // SELECT [product].[Name], [product].[ListPrice]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] > 0.0
            // ORDER BY [product].[Name]
        }

        internal static void SelectLocalEntity(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            IEnumerable<Product> products = source
                .Where(product => product.ListPrice > 1_000) // LINQ to Entities.
                .AsEnumerable() // Do nothing.
                .Select(product => new Product()
                {
                    ProductID = product.ProductID,
                    Name = product.Name
                }); // LINQ to Objects: Enumerable.Select>. Return a generator.
            products.WriteLines(product => $"{product.ProductID}: {product.Name}"); // Execute query.
            // SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] > 1000.0
        }

        #endregion

        #region Element

        internal static void First(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            string first = source
                .Select(product => product.Name)
                .First() // Execute query.
                .WriteLine();
            // SELECT TOP(1) [product].[Name]
            // FROM [Production].[Product] AS [product]
        }

        internal static void FirstOrDefault(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var firstOrDefault = source
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                .FirstOrDefault(product => product.ListPrice > 5000); // Execute query.
            firstOrDefault?.Name.WriteLine();
            // SELECT TOP(1) [product].[Name], [product].[ListPrice]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] > 5000.0
        }

        internal static void Last(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            Product last = source.Last(); // Execute query.
#if EF
            // NotSupportedException: LINQ to Entities does not recognize the method 'Product Last[Product](System.Linq.IQueryable`1[Product])' method, and this method cannot be translated into a store expression.
#else
            // SELECT [p].[ProductID], [p].[ListPrice], [p].[Name], [p].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [p]
#endif
            $"{last.Name}: {last.ListPrice}".WriteLine();
        }

        internal static void LastOrDefault(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var lastOrDefault = source
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                .LastOrDefault(product => product.ListPrice <= 0); // Execute query.
#if EF
            // NotSupportedException: LINQ to Entities does not recognize the method 'Product LastOrDefault[Product](System.Linq.IQueryable`1[Product], System.Linq.Expressions.Expression`1[System.Func`2[roduct,System.Boolean]])' method, and this method cannot be translated into a store expression.
#else
            // SELECT [product].[Name], [product].[ListPrice]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] <= 0.0
#endif
            (lastOrDefault == null).WriteLine(); // True
        }

        internal static void Single(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var single = source
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                .Single(product => product.ListPrice < 50); // Execute query.
            $"{single.Name}: {single.ListPrice}".WriteLine();
            // SELECT TOP(2) [product].[Name], [product].[ListPrice]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] < 50.0
        }

        internal static void SingleOrDefault(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var singleOrDefault = source
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                .SingleOrDefault(product => product.ListPrice < 1); // Execute query.
            singleOrDefault?.Name.WriteLine();
            // SELECT TOP(2) [product].[Name], [product].[ListPrice]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] < 1.0
        }

        internal static void ElementAt(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            Product elementAt = source.ElementAt(10); // Execute query.
            // NotSupportedException
        }

        internal static void ElementAtOrDefault(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            Product elementAt = source.ElementAtOrDefault(1_000); // Execute query.
            // NotSupportedException
        }

        #endregion

        #region Aggregate

        internal static void Count(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> source = adventureWorks.ProductCategories;
            int count = source.Count().WriteLine(); // Execute query.
            // SELECT COUNT(*)
            // FROM [Production].[ProductCategory] AS [p]
        }

        internal static void LongCount(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            long longCount = source.LongCount(product => product.ListPrice > 0).WriteLine(); // Execute query.
            // SELECT COUNT_BIG(*)
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] > 0.0
        }

        internal static void Max(AdventureWorks adventureWorks)
        {
            IQueryable<ProductPhoto> source = adventureWorks.ProductPhotos;
            DateTime max = source.Select(photo => photo.ModifiedDate).Max().WriteLine(); // Execute query.
            // SELECT MAX([photo].[ModifiedDate])
            // FROM [Production].[ProductPhoto] AS [photo]
        }

        internal static void Min(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            decimal min = source.Min(product => product.ListPrice).WriteLine(); // Execute query.
            // SELECT MIN([product].[ListPrice])
            // FROM [Production].[Product] AS [product]
        }

        internal static void Sum(AdventureWorks adventureWorks)
        {
            IQueryable<TransactionHistory> source = adventureWorks.Transactions;
            decimal sum = source.Sum(transaction => transaction.ActualCost).WriteLine(); // Execute query.
            // SELECT SUM([transaction].[ActualCost])
            // FROM [Production].[TransactionHistory] AS [transaction]
            // WHERE ([transaction].[TransactionType] = N'W') OR (([transaction].[TransactionType] = N'S') OR ([transaction].[TransactionType] = N'P'))
        }

        internal static void Average(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            decimal average = source.Select(product => product.ListPrice).Average().WriteLine(); // Execute query.
#if EF
            // SELECT 
            //    [GroupBy1].[A1] AS [C1]
            //    FROM ( SELECT 
            //        AVG([Extent1].[ListPrice]) AS [A1]
            //        FROM [Production].[Product] AS [Extent1]
            //    )  AS [GroupBy1]
#else
            // SELECT [product].[ListPrice]
            // FROM [Production].[Product] AS [product]
#endif
        }

        #endregion

        #region Quantifiers

        internal static void ContainsEntity(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            Product single = source.Single(product => product.ListPrice == 20.24M); // Execute query.
            // SELECT TOP(2) [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ListPrice] = 20.24
            bool contains = source
                .Where(product => product.ProductSubcategoryID == 7)
                .Contains(single).WriteLine(); // Execute query.
#if EF
            // NotSupportedException: Unable to create a constant value of type 'Product'. Only primitive types or enumeration types are supported in this context.
#else
            // SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[ProductSubcategoryID] = 7
            //.ArgumentException: Expression of type 'Tutorial.LinqToEntities.Product' cannot be used for parameter of type 'Microsoft.EntityFrameworkCore.Storage.ValueBuffer' of method 'Boolean Contains[ValueBuffer](System.Collections.Generic.IEnumerable`1[Microsoft.EntityFrameworkCore.Storage.ValueBuffer], Microsoft.EntityFrameworkCore.Storage.ValueBuffer)' Parameter name: arg1

#endif
        }

        internal static void ContainsPrimitive(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            bool contains = source
                .Select(product => product.ListPrice).Contains(100)
                .WriteLine(); // Execute query.
            // SELECT CASE
            //    WHEN EXISTS (
            //        SELECT 1
            //        FROM [Production].[Product] AS [product]
            //        WHERE [product].[ListPrice] = 100.0)
            //    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
            // END
        }

        internal static void Any(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            bool any = source.Any().WriteLine(); // Execute query.
            // SELECT CASE
            //    WHEN EXISTS (
            //        SELECT 1
            //        FROM [Production].[Product] AS [p])
            //    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
            // END
        }

        internal static void AnyWithPredicate(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            bool any = source.Any(product => product.ListPrice > 10).WriteLine(); // Execute query.
            // SELECT CASE
            //    WHEN EXISTS (
            //        SELECT 1
            //        FROM [Production].[Product] AS [product]
            //        WHERE [product].[ListPrice] > 10.0)
            //    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
            // END
        }

        internal static void AllWithPredicate(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            bool all = source.All(product => product.ListPrice > 10).WriteLine(); // Execute query.
            // SELECT CASE
            //    WHEN NOT EXISTS (
            //        SELECT 1
            //        FROM [Production].[Product] AS [product]
            //        WHERE [product].[ListPrice] <= 10.0)
            //    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
            // END
        }

        internal static void AllNot(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            bool allNot = source.All(product => product.ProductSubcategoryID != null).WriteLine(); // Execute query.
            // SELECT CASE
            //    WHEN NOT EXISTS (
            //        SELECT 1
            //        FROM [Production].[Product] AS [product]
            //        WHERE [product].[ProductSubcategoryID] IS NULL)
            //    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
            // END
        }

        internal static void NotAny(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            bool notAny = !source.Any(product => product.ProductSubcategoryID == null).WriteLine(); // Execute query.
            // SELECT CASE
            //    WHEN EXISTS (
            //        SELECT 1
            //        FROM [Production].[Product] AS [product]
            //        WHERE [product].[ProductSubcategoryID] IS NULL)
            //    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
            // END
        }

        #endregion

        #region Equality

        internal static void SequenceEqual(AdventureWorks adventureWorks)
        {
            IQueryable<Product> highPriceProducts = adventureWorks.Products
                .Where(product => product.ListPrice > 2000)
                .OrderBy(product => product.Name);
            IQueryable<Product> touringBikes = adventureWorks.Products
                .Where(product => product.ProductSubcategoryID == 3)
                .OrderBy(product => product.Name);
            bool sequenceEqual = highPriceProducts.SequenceEqual(touringBikes).WriteLine();
            // NotSupportedException.
        }

        #endregion
    }
}

#if DEMO
namespace System.Linq
{
    using System.Collections.Generic;

    public static class Enumerable
    {
        public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source) => source;
    }

    public static class Queryable
    {
        public static IQueryable<TElement> AsQueryable<TElement>(this IEnumerable<TElement> source) =>
            source as IQueryable<TElement> queryable ?? new EnumerableQuery<TElement>(source);
    }
}
#endif
