namespace Tutorial.LinqToEntities
{
    using System.Linq;

    public static class QueryExpressions
    {
        #region Join

        internal static void InnerJoinWithJoin(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categorySubcategories =
                from category in outer
                join subcategory in inner
                on category.ProductCategoryID equals subcategory.ProductCategoryID
                select new { Category = category.Name, Subategory = subcategory.Name }; // Define query.
            categorySubcategories.WriteLines(); // Execute query.
        }

        internal static void InnerJoinWithSelectMany(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categorySubcategories =
                from category in outer
                from subcategory in inner
                where category.ProductCategoryID == subcategory.ProductCategoryID
                select new { Category = category.Name, Subategory = subcategory.Name }; // Define query.
            categorySubcategories.WriteLines(); // Execute query.
        }

        internal static void InnerJoinWithSelect(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categorySubcategories =
                from category in outer
                select new
                {
                    Category = category,
                    Subcategories = from subcategory in inner
                                    where category.ProductCategoryID == subcategory.ProductCategoryID
                                    select subcategory
                } into category
                from subcategory in category.Subcategories // LEFT OUTER JOIN if DefaultIfEmpty is called.
                select new { Category = category.Category.Name, Subcategory = subcategory.Name }; // Define query.
            categorySubcategories.WriteLines(); // Execute query.
        }

        internal static void InnerJoinWithSelectAndRelationship(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            var categorySubcategories =
                from category in outer
                select new { Category = category, Subcategories = category.ProductSubcategories } into category
                from subcategory in category.Subcategories
                select new { Category = category.Category.Name, Subcategory = subcategory.Name }; // Define query.
            categorySubcategories.WriteLines(); // Execute query.
        }

        internal static void InnerJoinWithSelectManyAndRelationship(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            var categorySubcategories =
                from category in outer
                from subcategory in category.ProductSubcategories
                select new { Category = category.Name, Subcategory = subcategory.Name }; // Define query.
            categorySubcategories.WriteLines(); // Execute query.
        }

        internal static void MultipleInnerJoinsWithRelationship(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            var products =
                from product in source
                from productProductPhoto in product.ProductProductPhotos
                select new
                {
                    Product = product.Name,
                    Photo = productProductPhoto.ProductPhoto.LargePhotoFileName
                }; // Define query.
            products.WriteLines(); // Execute query.
        }

        internal static void InnerJoinWithMultipleKeys(AdventureWorks adventureWorks)
        {
            IQueryable<Product> outer = adventureWorks.Products;
            IQueryable<TransactionHistory> inner = adventureWorks.Transactions;
            var transactions =
                from product in adventureWorks.Products
                join transaction in adventureWorks.Transactions
                on new { ProductID = product.ProductID, UnitPrice = product.ListPrice }
                    equals new { ProductID = transaction.ProductID, UnitPrice = transaction.ActualCost / transaction.Quantity }
                select new { Name = product.Name, Quantity = transaction.Quantity }; // Define query.
            transactions.WriteLines(); // Execute query.
        }

        internal static void InnerJoinWithGroupJoin(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categorySubcategories =
                from category in outer
                join subcategory in inner
                on category.ProductCategoryID equals subcategory.ProductCategoryID into subcategories
                from subcategory in subcategories // LEFT OUTER JOIN if DefaultIfEmpty is called.
                select new { Category = category.Name, Subategory = subcategory.Name }; // Define query.
            categorySubcategories.WriteLines(); // Execute query.
        }

        internal static void LeftOuterJoinWithGroupJoin(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categorySubcategories =
                from category in outer
                join subcategory in inner
                on category.ProductCategoryID equals subcategory.ProductCategoryID into subcategories
                select new { Category = category, Subcategories = subcategories }; // Define query.
            categorySubcategories.WriteLines(); // Execute query.
        }

        internal static void LeftOuterJoinWithGroupJoinAndSelectMany(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categories =
                from category in outer
                join subcategory in inner
                on category.ProductCategoryID equals subcategory.ProductCategoryID into subcategories
                from subcategory in subcategories.DefaultIfEmpty() // INNER JOIN if DefaultIfEmpty is missing.
                select new { Category = category.Name, Subcategory = subcategory.Name }; // Define query.
            categories.WriteLines(); // Execute query.
        }

        internal static void LeftOuterJoinWithSelect(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categorySubcategories =
                from category in outer
                select new
                {
                    Category = category,
                    Subcategories = from subcategory in inner
                                    where subcategory.ProductCategoryID == category.ProductCategoryID
                                    select subcategory
                } into category
                from subcategory in category.Subcategories.DefaultIfEmpty()
                select new { Category = category.Category.Name, Subcategory = subcategory.Name }; // Define query.
            categorySubcategories.WriteLines(); // Execute query.
        }

        internal static void LeftOuterJoinWithSelectMany(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = adventureWorks.ProductSubcategories;
            var categorySubcategories =
                from category in outer
                from subcategory in (from subcategory in inner
                                     where category.ProductCategoryID == subcategory.ProductCategoryID
                                     select subcategory).DefaultIfEmpty() // INNER JOIN if DefaultIfEmpty is missing.
                select new { Category = category.Name, Subcategory = subcategory.Name }; // Define query.
            categorySubcategories.WriteLines(); // Execute query.
        }

        internal static void LeftOuterJoinWithSelectRelationship(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> outer = adventureWorks.ProductCategories;
            var categorySubcategories =
                from category in outer
                select new { Category = category, Subcategories = category.ProductSubcategories } into category
                from subcategory in category.Subcategories.DefaultIfEmpty() // INNER JOIN if DefaultIfEmpty is missing.
                select new { Category = category.Category.Name, Subcategory = subcategory.Name }; // Define query.
            categorySubcategories.WriteLines(); // Execute query.
        }

        internal static void LeftOuterJoinWithSelectManyRelationship(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> source = adventureWorks.ProductCategories;
            var categorySubcategories =
                from category in source
                from subcategory in category.ProductSubcategories.DefaultIfEmpty() // INNER JOIN if DefaultIfEmpty is missing.
                select new { Category = category.Name, Subcategory = subcategory.Name }; // Define query.
            categorySubcategories.WriteLines(); // Execute query.
        }

        internal static void CrossJoinWithSelectMany(AdventureWorks adventureWorks)
        {
            IQueryable<Product> outer = from product in adventureWorks.Products
                                        where product.ListPrice > 2000
                                        select product;
            IQueryable<Product> inner = from product in adventureWorks.Products
                                        where product.ListPrice < 100
                                        select product; ;
            var bundles =
                from outerProduct in outer
                from innerProduct in inner
                select new { Expensive = outerProduct.Name, Cheap = innerProduct.Name }; // Define query.
            bundles.WriteLines(bundle => $"{bundle.Expensive}: {bundle.Cheap}"); // Execute query.
        }

        internal static void CrossJoinWithJoin(AdventureWorks adventureWorks)
        {
            IQueryable<Product> outer = from product in adventureWorks.Products
                                        where product.ListPrice > 2000
                                        select product;
            IQueryable<Product> inner = from product in adventureWorks.Products
                                        where product.ListPrice < 100
                                        select product; ;
            var bundles =
                from outerProduct in outer
                join innerProduct in inner
                on 1 equals 1
                select new { Expensive = outerProduct.Name, Cheap = innerProduct.Name }; // Define query.
            bundles.WriteLines(bundle => $"{bundle.Expensive}: {bundle.Cheap}"); // Execute query.
        }

        internal static void SelfJoin(AdventureWorks adventureWorks)
        {
            IQueryable<Product> outer = adventureWorks.Products;
            IQueryable<Product> inner = adventureWorks.Products;
            var products =
                from outerProduct in outer
                join innerProduct in inner
                on outerProduct.ListPrice equals innerProduct.ListPrice into samePriceProducts
                select new
                {
                    Name = outerProduct.Name,
                    ListPrice = outerProduct.ListPrice,
                    SamePriceProducts = from samePriceProduct in samePriceProducts
                                        where samePriceProduct.ProductID != outerProduct.ProductID
                                        select samePriceProduct.Name
                }; // Define query.
            products.WriteLines(product =>
                $"{product.Name} ({product.ListPrice}): {string.Join(", ", product.SamePriceProducts)}"); // Execute query.
        }

        #endregion
    }
}
