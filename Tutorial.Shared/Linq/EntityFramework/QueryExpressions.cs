namespace Dixin.Linq.EntityFramework
{
    using System.Linq;

    public static class QueryExpressions
    {
        #region Join

        internal static void InnerJoinWithJoin(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> outer = adventureWorks.Suppliers;
            IQueryable<SupplierCategory> inner = adventureWorks.SupplierCategories;
            var subcategories =
                from subcategory in outer
                join category in inner
                on subcategory.SupplierCategoryID equals category.SupplierCategoryID
                select new { Subcategory = subcategory.SupplierName, Category = category.SupplierCategoryName }; // Define query.
            subcategories.WriteLines(subcategory => 
                $"{subcategory.Category}: {subcategory.Subcategory}"); // Execute query.
        }

        internal static void InnerJoinWithSelectMany(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> outer = adventureWorks.Suppliers;
            IQueryable<SupplierCategory> inner = adventureWorks.SupplierCategories;
            var subcategories =
                from subcategory in outer
                from category in inner
                where subcategory.SupplierCategoryID == category.SupplierCategoryID
                select new { Subcategory = subcategory.SupplierName, Category = category.SupplierCategoryName }; // Define query.
            subcategories.WriteLines(subcategory => 
                $"{subcategory.Category}: {subcategory.Subcategory}"); // Execute query.
        }

        internal static void InnerJoinWithGroupJoin(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> outer = adventureWorks.Suppliers;
            IQueryable<SupplierCategory> inner = adventureWorks.SupplierCategories;
            var subcategories =
                from subcategory in outer
                join category in inner
                on subcategory.SupplierCategoryID equals category.SupplierCategoryID into categories
                from category in categories // LEFT OUTER JOIN if DefaultIfEmpty is called.
                select new { Subcategory = subcategory.SupplierName, Category = category.SupplierCategoryName }; // Define query.
            subcategories.WriteLines(subcategory => 
                $"{subcategory.Category}: {subcategory.Subcategory}"); // Execute query.
        }

        internal static void InnerJoinWithSelect(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> outer = adventureWorks.Suppliers;
            IQueryable<SupplierCategory> inner = adventureWorks.SupplierCategories;
            var categories =
                from subcategory in outer
                select new
                {
                    Subcategory = subcategory,
                    Categories = from category in inner
                                 where category.SupplierCategoryID == subcategory.SupplierCategoryID
                                 select category
                } into subcategory
                from category in subcategory.Categories // LEFT OUTER JOIN if DefaultIfEmpty is called.
                select new { Subcategory = subcategory.Subcategory.SupplierName, Category = category.SupplierCategoryName }; // Define query.
            categories.WriteLines(category => 
                $"{category.Category}: {category.Subcategory}"); // Execute query.
        }

        internal static void InnerJoinWithAssociation(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> outer = adventureWorks.Suppliers;
            var subcategories =
                from subcategory in outer
                select new { Subcategory = subcategory.SupplierName, Category = subcategory.SupplierCategory.SupplierCategoryName }; // Define query.
            subcategories.WriteLines(subcategory => 
                $"{subcategory.Category}: {subcategory.Subcategory}"); // Execute query.
        }

        internal static void MultipleInnerJoinsWithAssociations(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            var products =
                from product in source
                from productProductPhoto in product.StockItemStockGroups
                select new
                {
                    Product = product.StockItemName,
                    Photo = productProductPhoto.StockGroup.StockGroupName
                }; // Define query.
            products.WriteLines(product => $"{product.Product}: {product.Photo}"); // Execute query.
        }

        internal static void InnerJoinWithMultipleKeys(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> outer = adventureWorks.Suppliers;
            IQueryable<SupplierCategory> inner = adventureWorks.SupplierCategories;
            var subcategories =
                from subcategory in outer
                join category in inner
                on new { Id = subcategory.SupplierCategoryID, FirstLetter = subcategory.SupplierName.Substring(0, 1) }
                    equals new { Id = category.SupplierCategoryID, FirstLetter = category.SupplierCategoryName.Substring(0, 1) }
                select new { Subcategory = subcategory.SupplierName, Category = category.SupplierCategoryName }; // Define query.
            subcategories.WriteLines(subcategory => $"{subcategory.Category}: {subcategory.Subcategory}"); // Execute query.
        }

        internal static void LeftOuterJoinWithGroupJoin(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> outer = adventureWorks.SupplierCategories;
            IQueryable<Supplier> inner = adventureWorks.Suppliers;
            var categories =
                from category in outer
                join subcategory in inner
                on category.SupplierCategoryID equals subcategory.SupplierCategoryID into subcategories
                select new
                {
                    Category = category.SupplierCategoryName,
                    Subcategories = subcategories.Select(subcategory => subcategory.SupplierName)
                }; // Define query.
            categories.WriteLines(category => 
                $"{category.Category}: {string.Join(", ", category.Subcategories)}"); // Execute query.
        }

        internal static void LeftOuterJoinWithSelect(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> outer = adventureWorks.SupplierCategories;
            IQueryable<Supplier> inner = adventureWorks.Suppliers;
            var categories =
                from category in outer
                select new
                {
                    Category = category,
                    Subcategories = from subcategory in inner
                                    where subcategory.SupplierCategoryID == category.SupplierCategoryID
                                    select subcategory
                }; // Define query.
            categories.WriteLines(category => 
                $"{category.Category}: {string.Join(", ", category.Subcategories)}"); // Execute query.
        }

        internal static void LeftOuterJoinWithGroupJoinAndSelectMany(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> outer = adventureWorks.SupplierCategories;
            IQueryable<Supplier> inner = adventureWorks.Suppliers;
            var categories =
                from category in outer
                join subcategory in inner
                on category.SupplierCategoryID equals subcategory.SupplierCategoryID into subcategories
                from subcategory in subcategories.DefaultIfEmpty() // INNER JOIN if DefaultIfEmpty is missing.
                select new { Category = category.SupplierCategoryName, Subcategory = subcategory.SupplierName }; // Define query.
            categories.WriteLines(category => 
                $"{category.Category}: {category.Subcategory}"); // Execute query.
        }

        internal static void LeftOuterJoinWithSelectAndSelectMany(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> outer = adventureWorks.SupplierCategories;
            IQueryable<Supplier> inner = adventureWorks.Suppliers;
            var categories =
                from category in outer
                select new
                {
                    Category = category,
                    Subcategories = from subcategory in inner
                                    where subcategory.SupplierCategoryID == category.SupplierCategoryID
                                    select subcategory
                } into category
                from subcategory in category.Subcategories.DefaultIfEmpty() // INNER JOIN if DefaultIfEmpty is missing.
                select new { Category = category.Category.SupplierCategoryName, Subcategory = subcategory.SupplierName }; // Define query.
            categories.WriteLines(category => 
                $"{category.Category}: {category.Subcategory}"); // Execute query.
        }

        internal static void LeftOuterJoinWithAssociation(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories;
            var categories = source.SelectMany(
                category => category.Suppliers.DefaultIfEmpty(), // INNER JOIN if DefaultIfEmpty is missing.
                (category, subcategory) =>
                    new { Category = category.SupplierCategoryName, Subcategory = subcategory.SupplierName }); // Define query.
            categories.WriteLines(subcategory => 
                $"{subcategory.Category}: {subcategory.Subcategory}"); // Execute query.
        }

        internal static void CrossJoinWithSelectMany(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> outer = adventureWorks.StockItems.Where(product => product.UnitPrice > 2000);
            IQueryable<StockItem> inner = adventureWorks.StockItems.Where(product => product.UnitPrice < 100);
            var bundles =
                from outerProduct in outer
                from innerProduct in inner
                    // where true == true
                select new { Expensive = outerProduct.StockItemName, Cheap = innerProduct.StockItemName }; // Define query.
            bundles.WriteLines(bundle => $"{bundle.Expensive}: {bundle.Cheap}"); // Execute query.
        }

        internal static void CrossJoinWithJoin(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> outer = adventureWorks.StockItems.Where(product => product.UnitPrice > 2000);
            IQueryable<StockItem> inner = adventureWorks.StockItems.Where(product => product.UnitPrice < 100);
            var bundles =
                from outerProduct in outer
                join innerProduct in inner
                on true equals true
                select new { Expensive = outerProduct.StockItemName, Cheap = innerProduct.StockItemName }; // Define query.
            bundles.WriteLines(bundle => $"{bundle.Expensive}: {bundle.Cheap}"); // Execute query.
        }

        internal static void SelfJoin(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> outer = adventureWorks.StockItems;
            IQueryable<StockItem> inner = adventureWorks.StockItems;
            var products =
                from outerProduct in outer
                join innerProduct in inner
                on outerProduct.UnitPrice equals innerProduct.UnitPrice into samePriceProducts
                select new
                {
                    Name = outerProduct.StockItemName,
                    ListPrice = outerProduct.UnitPrice,
                    SamePriceProducts = from samePriceProduct in samePriceProducts
                                        where samePriceProduct.StockItemID != outerProduct.StockItemID
                                        select samePriceProduct.StockItemName
                }; // Define query.
            products.WriteLines(product => 
                $"{product.Name} ({product.ListPrice}): {string.Join(", ", product.SamePriceProducts)}"); // Execute query.
        }

        #endregion
    }
}
