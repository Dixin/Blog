namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static partial class QueryMethods
    {
        #region Generation

        internal static void DefaultIfEmpty(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories;
            IQueryable<SupplierCategory> categories = source.DefaultIfEmpty(); // Define query.
            categories.WriteLines(category => category?.SupplierCategoryName); // Execute query.
        }

        internal static void DefaultIfEmptyWithPrimitive(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories;
            IQueryable<int> categories = source
                .Select(category => category.SupplierCategoryID)
                .DefaultIfEmpty(-1); // Define query.
            categories.WriteLines(); // Execute query.
        }

        internal static void DefaultIfEmptyWithEntity(WideWorldImporters adventureWorks)
        {
            SupplierCategory defaultCategory = new SupplierCategory();
            IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories;
            IQueryable<SupplierCategory> categories = source.DefaultIfEmpty(defaultCategory); // Define query.
            categories.WriteLines(category => category?.SupplierCategoryName); // Execute query.
#if NETFX
            // NotSupportedException: Unable to create a constant value of type 'Dixin.Linq.EntityFramework.ProductCategory'. Only primitive types or enumeration types are supported in this context.
#endif
        }

        #endregion

        #region Filtering

        internal static void Where(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories;
            IQueryable<SupplierCategory> categories = source.Where(category => category.SupplierCategoryID > 0); // Define query.
            categories.WriteLines(category => category.SupplierCategoryName); // Execute query.
        }

        internal static void WhereWithOr(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories;
            IQueryable<SupplierCategory> categories = source.Where(category =>
                category.SupplierCategoryID <= 1 || category.SupplierCategoryID >= 4); // Define query.
            categories.WriteLines(category => category.SupplierCategoryName); // Execute query.
        }

        internal static void WhereWithAnd(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories;
            IQueryable<SupplierCategory> categories = source.Where(category =>
                category.SupplierCategoryID > 0 && category.SupplierCategoryID < 5); // Define query.
            categories.WriteLines(category => category.SupplierCategoryName); // Execute query.
        }

        internal static void WhereAndWhere(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories;
            IQueryable<SupplierCategory> categories = source
                .Where(category => category.SupplierCategoryID > 0)
                .Where(category => category.SupplierCategoryID < 5); // Define query.
            categories.WriteLines(category => category.SupplierCategoryName); // Execute query.
        }

        internal static void WhereWithIs(WideWorldImporters adventureWorks)
        {
            IQueryable<Country> source = adventureWorks.Countries;
            IQueryable<Country> transactions = source.Where(transaction => transaction is EuropeCountry); // Define query.
            transactions.WriteLines(transaction => $"{transaction.GetType().Name} {transaction.CountryName} {transaction.LatestRecordedPopulation}"); // Execute query.
        }

        internal static void OfTypeWithEntity(WideWorldImporters adventureWorks)
        {
            IQueryable<Country> source = adventureWorks.Countries;
            IQueryable<EuropeCountry> transactions = source.OfType<EuropeCountry>(); // Define query.
            transactions.WriteLines(transaction => $"{transaction.GetType().Name} {transaction.CountryName} {transaction.LatestRecordedPopulation}"); // Execute query.
        }

        internal static void OfTypeWithPrimitive(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            IQueryable<int> products = source.Select(p => p.SupplierID).OfType<int>(); // Define query.
            products.WriteLines(); // Execute query.
#if NETFX
            // NotSupportedException: 'System.Int32' is not a valid metadata type for type filtering operations. Type filtering is only valid on entity types and complex types.
#endif
        }

        #endregion

        #region Mapping

        internal static void Select(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories;
            IQueryable<string> categories = source.Select(category =>
                category.SupplierCategoryName + category.SupplierCategoryName); // Define query.
            categories.WriteLines(); // Execute query.
        }

        internal static void SelectWithStringConcat(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories;
            IQueryable<string> categories = source.Select(category =>
                string.Concat(category.SupplierCategoryName, category.SupplierCategoryName)); // Define query.
            categories.WriteLines(); // Execute query.
        }

        internal static void SelectAnonymousType(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            var products = source.Select(product =>
                new { Name = product.StockItemName, IsExpensive = product.UnitPrice > 1000, Constant = 1 }); // Define query.
            products.WriteLines(product => product.Name); // Execute query.
        }

        #endregion

        #region Grouping

        internal static void GroupBy(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> source = adventureWorks.Suppliers;
            IQueryable<IGrouping<int, string>> groups = source.GroupBy(
                subcategory => subcategory.SupplierCategoryID,
                subcategory => subcategory.SupplierName); // Define query.
            groups.WriteLines(group => $"{group.Key}: {string.Join(", ", group)}"); // Execute query.
        }

        internal static void GroupByWithResultSelector(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> source = adventureWorks.Suppliers;
            var groups = source.GroupBy(
                subcategory => subcategory.SupplierCategoryID,
                subcategory => subcategory.SupplierName,
                (key, group) => new { CategoryID = key, SubcategoryCount = group.Count() }); // Define query.
            groups.WriteLines(group => $"{group.CategoryID}: {group.SubcategoryCount}"); // Execute query.
        }

        internal static void GroupByAndSelect(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> source = adventureWorks.Suppliers;
            var groups = source
                .GroupBy(
                    subcategory => subcategory.SupplierCategoryID,
                    subcategory => subcategory.SupplierName)
                .Select(group => new { CategoryID = group.Key, SubcategoryCount = group.Count() }); // Define query.
            groups.WriteLines(group => $"{group.CategoryID}: {group.SubcategoryCount}"); // Execute query.
        }

        internal static void GroupByAndSelectMany(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> source = adventureWorks.Suppliers;
            IQueryable<Supplier> distinct = source
                .GroupBy(subcategory => subcategory.SupplierCategoryID)
                .SelectMany(group => group); // Define query.
            distinct.WriteLines(subcategory => subcategory.SupplierName); // Execute query.
        }

        internal static void GroupByMultipleKeys(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            var groups = source.GroupBy(
                product => new { ProductSubcategoryID = product.SupplierID, ListPrice = product.UnitPrice },
                (key, group) => new
                {
                    ProductSubcategoryID = key.ProductSubcategoryID,
                    ListPrice = key.ListPrice,
                    Count = group.Count()
                }); // Define query.
            groups.WriteLines(group =>
                $"{group.ProductSubcategoryID}, {group.ListPrice}: {group.Count}"); // Execute query.
        }

        #endregion

        #region Join

        internal static void InnerJoinWithJoin(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> outer = adventureWorks.Suppliers;
            IQueryable<SupplierCategory> inner = adventureWorks.SupplierCategories;
            var subcategories = outer.Join(
                inner,
                subcategory => subcategory.SupplierCategoryID,
                category => category.SupplierCategoryID,
                (subcategory, category) => new { Subcategory = subcategory.SupplierName, Category = category.SupplierCategoryName }); // Define query.
            subcategories.WriteLines(subcategory =>
                $"{subcategory.Category}: {subcategory.Subcategory}"); // Execute query.
        }

        internal static void InnerJoinWithSelectMany(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> outer = adventureWorks.Suppliers;
            IQueryable<SupplierCategory> inner = adventureWorks.SupplierCategories;
            var subcategories = outer
                .SelectMany(
                    subcategory => inner,
                    (subcategory, category) => new { Subcategory = subcategory, Category = category })
                .Where(crossJoinValue =>
                    crossJoinValue.Subcategory.SupplierCategoryID == crossJoinValue.Category.SupplierCategoryID)
                .Select(crossJoinValue =>
                    new { Subcategory = crossJoinValue.Subcategory.SupplierName, Category = crossJoinValue.Category.SupplierCategoryName }); // Define query.
            subcategories.WriteLines(subcategory =>
                $"{subcategory.Category}: {subcategory.Subcategory}"); // Execute query.
        }

        internal static void InnerJoinWithGroupJoin(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> outer = adventureWorks.Suppliers;
            IQueryable<SupplierCategory> inner = adventureWorks.SupplierCategories;
            var subcategories = outer
                .GroupJoin(
                    inner,
                    subcategory => subcategory.SupplierCategoryID,
                    category => category.SupplierCategoryID,
                    (subcategory, categories) => new { Subcategory = subcategory, Categories = categories })
                .SelectMany(
                    subcategory => subcategory.Categories, // LEFT OUTER JOIN if DefaultIfEmpty is called.
                    (subcategory, category) =>
                        new { Subcategory = subcategory.Subcategory.SupplierName, Category = category.SupplierCategoryName }); // Define query.
            subcategories.WriteLines(subcategory =>
                $"{subcategory.Category}: {subcategory.Subcategory}"); // Execute query.
        }

        internal static void InnerJoinWithSelect(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> outer = adventureWorks.Suppliers;
            IQueryable<SupplierCategory> inner = adventureWorks.SupplierCategories;
            var categories = outer
                .Select(subcategory => new
                {
                    Subcategory = subcategory,
                    Categories = inner.Where(category => category.SupplierCategoryID == subcategory.SupplierCategoryID)
                })
                .SelectMany(
                    subcategory => subcategory.Categories, // LEFT OUTER JOIN if DefaultIfEmpty is called.
                    (subcategory, category) =>
                        new { Subcategory = subcategory.Subcategory.SupplierName, Category = category.SupplierCategoryName }); // Define query.
            categories.WriteLines(category =>
                $"{category.Category}: {category.Subcategory}"); // Execute query.
        }

        internal static void InnerJoinWithAssociation(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> outer = adventureWorks.Suppliers;
            var subcategories = outer.Select(subcategory =>
                new { Subcategory = subcategory.SupplierName, Category = subcategory.SupplierCategory.SupplierCategoryName }); // Define query.
            subcategories.WriteLines(subcategory =>
                $"{subcategory.Category}: {subcategory.Subcategory}"); // Execute query.
        }

        internal static void MultipleInnerJoinsWithAssociations(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            var products = source.SelectMany(
                product => product.StockItemStockGroups,
                (product, productProductPhoto) => new
                {
                    Product = product.StockItemName,
                    Photo = productProductPhoto.StockGroup.StockGroupName
                }); // Define query.
            products.WriteLines(product => $"{product.Product}: {product.Photo}"); // Execute query.
        }

        internal static void InnerJoinWithMultipleKeys(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> outer = adventureWorks.Suppliers;
            IQueryable<SupplierCategory> inner = adventureWorks.SupplierCategories;
            var subcategories = outer.Join(
                inner,
                subcategory =>
                    new { ProductCategoryID = subcategory.SupplierCategoryID, Name = subcategory.SupplierName },
                category =>
                    new { ProductCategoryID = category.SupplierCategoryID, Name = category.SupplierCategoryName },
                (subcategory, category) => new { Subcategory = subcategory.SupplierName, Category = category.SupplierCategoryName }); // Define query.
            subcategories.WriteLines(subcategory => $"{subcategory.Category}: {subcategory.Subcategory}"); // Execute query.
        }

        internal static void LeftOuterJoinWithGroupJoin(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> outer = adventureWorks.SupplierCategories;
            IQueryable<Supplier> inner = adventureWorks.Suppliers;
            var categories = outer.GroupJoin(
                inner,
                category => category.SupplierCategoryID,
                subcategory => subcategory.SupplierCategoryID,
                (category, subcategories) => new
                {
                    Category = category.SupplierCategoryName,
                    Subcategories = subcategories.Select(subcategory => subcategory.SupplierName)
                }); // Define query.
            categories.WriteLines(category =>
                $"{category.Category}: {string.Join(", ", category.Subcategories)}"); // Execute query.
        }

        internal static void LeftOuterJoinWithSelect(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> outer = adventureWorks.SupplierCategories;
            IQueryable<Supplier> inner = adventureWorks.Suppliers;
            var categories = outer
                .Select(category => new
                {
                    Category = category.SupplierCategoryName,
                    Subcategories = inner
                        .Where(subcategory => subcategory.SupplierCategoryID == category.SupplierCategoryID)
                        .Select(subcategory => subcategory.SupplierName)
                }); // Define query.
            categories.WriteLines(category =>
                $"{category.Category}: {string.Join(", ", category.Subcategories)}"); // Execute query.
        }

        internal static void LeftOuterJoinWithGroupJoinAndSelectMany(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> outer = adventureWorks.SupplierCategories;
            IQueryable<Supplier> inner = adventureWorks.Suppliers;
            var categories = outer
                .GroupJoin(
                    inner,
                    category => category.SupplierCategoryID,
                    subcategory => subcategory.SupplierCategoryID,
                    (category, subcategories) => new { Category = category, Subcategories = subcategories })
                .SelectMany
                    (category => category.Subcategories.DefaultIfEmpty(), // INNER JOIN if DefaultIfEmpty is missing.
                    (category, subcategory) =>
                        new { Category = category.Category.SupplierCategoryName, Subcategory = subcategory.SupplierName }); // Define query.
            categories.WriteLines(category =>
                $"{category.Category}: {category.Subcategory}"); // Execute query.
        }

        internal static void LeftOuterJoinWithSelectAndSelectMany(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> outer = adventureWorks.SupplierCategories;
            IQueryable<Supplier> inner = adventureWorks.Suppliers;
            var categories = outer
                .Select(category => new
                {
                    Category = category,
                    Subcategories = inner
                        .Where(subcategory => subcategory.SupplierCategoryID == category.SupplierCategoryID)
                })
                .SelectMany(
                    category => category.Subcategories.DefaultIfEmpty(), // INNER JOIN if DefaultIfEmpty is missing.
                    (category, subcategory) =>
                        new { Category = category.Category.SupplierCategoryName, Subcategory = subcategory.SupplierName }); // Define query.
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
            var bundles = outer.SelectMany(
                outerProduct => inner,
                (outerProduct, innerProduct) =>
                    new { Expensive = outerProduct.StockItemName, Cheap = innerProduct.StockItemName }); // Define query.
            bundles.WriteLines(bundle => $"{bundle.Expensive}: {bundle.Cheap}"); // Execute query.
        }

        internal static void CrossJoinWithJoin(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> outer = adventureWorks.StockItems.Where(product => product.UnitPrice > 2000);
            IQueryable<StockItem> inner = adventureWorks.StockItems.Where(product => product.UnitPrice < 100);
            var bundles = outer.Join(
                inner,
                product => true,
                product => true,
                (outerProduct, innerProduct) =>
                    new { Expensive = outerProduct.StockItemName, Cheap = innerProduct.StockItemName }); // Define query.
            bundles.WriteLines(bundle => $"{bundle.Expensive}: {bundle.Cheap}"); // Execute query.
        }

        internal static void SelfJoin(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> outer = adventureWorks.StockItems;
            IQueryable<StockItem> inner = adventureWorks.StockItems;
            var products = outer.GroupJoin(
                inner,
                product => product.UnitPrice,
                product => product.UnitPrice,
                (product, samePriceProducts) => new
                {
                    Name = product.StockItemName,
                    ListPrice = product.UnitPrice,
                    SamePriceProducts = samePriceProducts
                        .Where(samePriceProduct => samePriceProduct.StockItemID != product.StockItemID)
                        .Select(samePriceProduct => samePriceProduct.StockItemName)
                }); // Define query.
            products.WriteLines(product =>
                $"{product.Name} ({product.ListPrice}): {string.Join(", ", product.SamePriceProducts)}"); // Execute query.
        }

        #endregion

        #region Apply

        internal static void CrossApplyWithGroupByAndTake(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> source = adventureWorks.Suppliers;
            var categories = source
                .GroupBy(subcategory => subcategory.SupplierCategoryID)
                .SelectMany(
                    group => group.Take(1),
                    (group, subcategory) =>
                        new { ProductCategoryID = group.Key, FirstSubcategory = subcategory }); // Define query.
            categories.WriteLines(category =>
                $"{category.ProductCategoryID}: {category.FirstSubcategory?.SupplierName}"); // Execute query.
        }

        internal static void CrossApplyWithGroupJoinAndTake(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> outer = adventureWorks.SupplierCategories;
            IQueryable<Supplier> inner = adventureWorks.Suppliers;
            var categories = outer
                .GroupJoin(
                    inner,
                    category => category.SupplierCategoryID,
                    subcategory => subcategory.SupplierCategoryID,
                    (category, subcategories) => new { Category = category, Subcategories = subcategories })
                .SelectMany(
                    category => category.Subcategories.Take(1),
                    (category, subcategory) =>
                        new { Category = category.Category, FirstSubcategory = subcategory }); // Define query.
            categories.WriteLines(category =>
                $"{category.Category.SupplierCategoryName}: {category.FirstSubcategory?.SupplierName}"); // Execute query.
        }

        internal static void CrossApplyWithAssociationAndTake(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories;
            var categories = source
                .Select(category => new { Category = category, Subcategories = category.Suppliers })
                .SelectMany(
                    category => category.Subcategories.Take(1),
                    (category, subcategory) =>
                        new { Category = category.Category, FirstSubcategory = subcategory }); // Define query.
            categories.WriteLines(category =>
                $"{category.Category.SupplierCategoryName}: {category.FirstSubcategory?.SupplierName}"); // Execute query.
        }

        internal static void OuterApplyWithGroupByAndFirstOrDefault(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> source = adventureWorks.Suppliers;
            var categories = source.GroupBy(
                subcategory => subcategory.SupplierCategoryID,
                (key, group) =>
                    new { ProductCategoryID = key, FirstSubcategory = group.FirstOrDefault() }); // Define query.
            categories.WriteLines(category =>
                $"{category.ProductCategoryID}: {category.FirstSubcategory?.SupplierName}"); // Execute query.
        }

        internal static void OuterApplyWithGroupJoinAndFirstOrDefault(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> outer = adventureWorks.SupplierCategories;
            IQueryable<Supplier> inner = adventureWorks.Suppliers;
            var categories = outer.GroupJoin(
                inner,
                category => category.SupplierCategoryID,
                subcategory => subcategory.SupplierCategoryID,
                (category, subcategories) =>
                    new { Category = category, FirstSubcategory = subcategories.FirstOrDefault() }); // Define query.
            categories.WriteLines(category =>
                $"{category.Category.SupplierCategoryName}: {category.FirstSubcategory?.SupplierName}"); // Execute query.
        }

        internal static void OuterApplyWithAssociationAndFirstOrDefault(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories;
            var categories = source.Select(category => new
            {
                Category = category,
                FirstSubcategory = category.Suppliers.FirstOrDefault()
            }); // Define query.
            categories.WriteLines(category =>
                $"{category.Category.SupplierCategoryName}: {category.FirstSubcategory?.SupplierName}"); // Execute query.
        }

        #endregion

        #region Concatenation

        internal static void Concat(WideWorldImporters adventureWorks)
        {
            IQueryable<string> first = adventureWorks.StockItems
                .Where(product => product.UnitPrice < 100)
                .Select(product => product.StockItemName);
            IQueryable<string> second = adventureWorks.StockItems
                .Where(product => product.UnitPrice > 2000)
                .Select(product => product.StockItemName);
            IQueryable<string> concat = first.Concat(second); // Define query.
            concat.WriteLines(); // Execute query.
        }

        internal static void ConcatWithSelect(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> first = adventureWorks.StockItems.Where(product => product.UnitPrice < 100);
            IQueryable<StockItem> second = adventureWorks.StockItems.Where(product => product.UnitPrice > 2000);
            IQueryable<string> concat = first
                .Concat(second)
                .Select(product => product.StockItemName); // Define query.
            concat.WriteLines(); // Execute query.
        }

        #endregion

        #region Set

        internal static void Distinct(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> source = adventureWorks.Suppliers;
            IQueryable<int> distinct = source
                .Select(subcategory => subcategory.SupplierCategoryID)
                .Distinct(); // Define query.
            distinct.WriteLines(); // Execute query.
        }

        internal static void DistinctWithGroupBy(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> source = adventureWorks.Suppliers;
            IQueryable<int> distinct = source.GroupBy(
                subcategory => subcategory.SupplierCategoryID,
                (key, group) => key); // Define query.
            distinct.WriteLines(); // Execute query.
        }

        internal static void DistinctMultipleKeys(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> source = adventureWorks.Suppliers;
            var distinct = source
                .Select(subcategory =>
                    new { ProductCategoryID = subcategory.SupplierCategoryID, Name = subcategory.SupplierName })
                .Distinct(); // Define query.
            distinct.WriteLines(subcategory => $"{subcategory.ProductCategoryID}: {subcategory.Name}"); // Execute query.
        }

        internal static void DistinctMultipleKeysWithGroupBy(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> source = adventureWorks.Suppliers;
            var distinct = source.GroupBy(
                subcategory => new { ProductCategoryID = subcategory.SupplierCategoryID, Name = subcategory.SupplierName },
                (key, group) => key); // Define query.
            distinct.WriteLines(subcategory => $"{subcategory.ProductCategoryID}: {subcategory.Name}"); // Execute query.
        }

        internal static void DistinctWithGroupByAndSelectAndFirstOrDefault(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> source = adventureWorks.Suppliers;
            IQueryable<string> distinct = source.GroupBy(
                subcategory => subcategory.SupplierCategoryID,
                (key, group) => group.Select(subcategory => subcategory.SupplierName).FirstOrDefault()); // Define query.
            distinct.WriteLines(); // Execute query.
        }

        internal static void Intersect(WideWorldImporters adventureWorks)
        {
            var first = adventureWorks.StockItems
                .Where(product => product.UnitPrice > 100)
                .Select(product => new { Name = product.StockItemName, ListPrice = product.UnitPrice });
            var second = adventureWorks.StockItems
                .Where(product => product.UnitPrice < 2000)
                .Select(product => new { Name = product.StockItemName, ListPrice = product.UnitPrice });
            var intersect = first.Intersect(second); // Define query.
            intersect.WriteLines(); // Execute query.
        }

        internal static void Except(WideWorldImporters adventureWorks)
        {
            var first = adventureWorks.StockItems
                .Where(product => product.UnitPrice > 100)
                .Select(product => new { Name = product.StockItemName, ListPrice = product.UnitPrice });
            var second = adventureWorks.StockItems
                .Where(product => product.UnitPrice > 2000)
                .Select(product => new { Name = product.StockItemName, ListPrice = product.UnitPrice });
            var except = first.Except(second); // Define query.
            except.WriteLines(); // Execute query.
        }

        #endregion

        #region Partitioning

        internal static void Skip(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            IQueryable<string> names = source
                .Skip(10)
                .Select(product => product.StockItemName); // Define query.
            names.WriteLines(); // Execute query.
#if NETFX
            // NotSupportedException: The method 'Skip' is only supported for sorted input in LINQ to Entities. The method 'OrderBy' must be called before the method 'Skip'.
#endif
        }

        internal static void OrderByAndSkip(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            IQueryable<string> products = source
                .OrderBy(product => product.StockItemName)
                .Skip(10)
                .Select(product => product.StockItemName); // Define query.
            products.WriteLines(); // Execute query.
        }

        internal static void Take(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            IQueryable<string> products = source
                .Take(10)
                .Select(product => product.StockItemName); // Define query.
            products.WriteLines(); // Execute query.
        }

        internal static void OrderByAndSkipAndTake(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            IQueryable<string> products = source
                .OrderBy(product => product.StockItemName)
                .Skip(20)
                .Take(10)
                .Select(product => product.StockItemName); // Define query.
            products.WriteLines(); // Execute query.
        }

        #endregion

        #region Ordering

        internal static void OrderBy(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            var products = source
                .OrderBy(product => product.UnitPrice)
                .Select(product => new { Name = product.StockItemName, ListPrice = product.UnitPrice }); // Define query.
            products.WriteLines(product => $"{product.Name}: {product.ListPrice}"); // Execute query.
        }

        internal static void OrderByDescending(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            var products = source
                .OrderByDescending(product => product.UnitPrice)
                .Select(product => new { Name = product.StockItemName, ListPrice = product.UnitPrice }); // Define query.
            products.WriteLines(product => $"{product.Name}: {product.ListPrice}"); // Execute query.
        }

        internal static void OrderByAndThenBy(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            var products = source
                .OrderBy(product => product.UnitPrice)
                .ThenBy(product => product.StockItemName)
                .Select(product => new { Name = product.StockItemName, ListPrice = product.UnitPrice }); // Define query.
            products.WriteLines(product => $"{product.Name}: {product.ListPrice}"); // Execute query.
        }

        internal static void OrderByAnonymousType(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            var products = source
                .OrderBy(product => new { ListPrice = product.UnitPrice, Name = product.StockItemName })
                .Select(product => new { Name = product.StockItemName, ListPrice = product.UnitPrice }); // Define query.
            products.WriteLines(product => $"{product.Name}: {product.ListPrice}"); // Execute query.
        }

        internal static void OrderByAndOrderBy(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            var products = source
                .OrderBy(product => product.UnitPrice)
                .OrderBy(product => product.StockItemName)
                .Select(product => new { Name = product.StockItemName, ListPrice = product.UnitPrice }); // Define query.
            products.WriteLines(product => $"{product.Name}: {product.ListPrice}"); // Execute query.
        }

        internal static void Reverse(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            var products = source
                .OrderBy(product => product.UnitPrice)
                .Reverse()
                .Select(product => new { Name = product.StockItemName, ListPrice = product.UnitPrice }); // Define query.
            products.WriteLines(product => $"{product.Name}: {product.ListPrice}"); // Execute query.
#if NETFX
            // NotSupportedException: LINQ to Entities does not recognize the method 'System.Linq.IQueryable`1[Dixin.Linq.EntityFramework.Product] Reverse[Product](System.Linq.IQueryable`1[Dixin.Linq.EntityFramework.Product])' method, and this method cannot be translated into a store expression.
#else
            // System.NotImplementedException: Remotion.Linq.Clauses.ResultOperators.ReverseResultOperator
#endif
        }

        #endregion

        #region Conversion

        internal static void CastPrimitive(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            IQueryable<string> listPrices = source
                .Select(product => product.UnitPrice)
                .Cast<string>(); // Define query.
            listPrices.WriteLines(); // Execute query.
        }

        internal static void CastEntity(WideWorldImporters adventureWorks)
        {
            IQueryable<Country> source = adventureWorks.Countries;
            IQueryable<AsiaCountry> transactions = source
                .Where(product => product.LatestRecordedPopulation > 1_000_000_000L)
                .Cast<AsiaCountry>(); // Define query.
            transactions.WriteLines(transaction => $"{transaction.GetType().Name} {transaction.CountryName} {transaction.LatestRecordedPopulation}"); // Execute query.
            // NotSupportedException: Unable to cast the type 'Dixin.Linq.EntityFramework.Product' to type 'Dixin.Linq.EntityFramework.UniversalProduct'. LINQ to Entities only supports casting EDM primitive or enumeration types.
        }

        internal static void AsEnumerableAsQueryable(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source1 = adventureWorks.StockItems;
            var query1 = source1 // DbSet<T> object, derives from DbQuery<T>.
                .Select(product => new { Name = product.StockItemName, ListPrice = product.UnitPrice }) // Return DbQuery<T> object.
                .AsEnumerable() // Do nothing, directly return the input DbQuery<T> object.
                .AsQueryable() // Do nothing, directly return the input DbQuery<T> object.
                .Where(product => product.ListPrice > 0); // Continue LINQ to Entities query.
            query1.WriteLines(product => $"{product.Name}: {product.ListPrice}");

            IQueryable<StockItem> source2 = adventureWorks.StockItems;
            var query2 = source2 // DbSet<T> object, derives from DbQuery<T>.
                .Select(product => new { Name = product.StockItemName, ListPrice = product.UnitPrice }) // Return DbQuery<T> object.
                .AsEnumerable() // Do nothing, directly return the input DbQuery<T> object.
                .Select(product => product) // Enumerable.Select, returns a generator wrapping the input DbQuery<T> object.
                .AsQueryable() // Return an EnumerableQuery<T> object wrapping the input generator.
                .Where(product => product.ListPrice > 0); // No longer LINQ to Entities query on DbSet<T> or DbQuery<T>.
            query2.WriteLines(product => $"{product.Name}: {product.ListPrice}");
        }

        internal static void SelectEntities(WideWorldImporters adventureWorks)
        {
            IQueryable<Country> source = adventureWorks.Countries;
            IQueryable<Country> transactions = source
                .Where(transaction => transaction is EuropeCountry)
                .Select(transaction => new EuropeCountry()
                {
                    CountryID = transaction.CountryID,
                    CountryName = transaction.CountryName,
                    LatestRecordedPopulation = transaction.LatestRecordedPopulation,
                }); // Define query.
            transactions.WriteLines(transaction => $"{transaction.GetType().Name} {transaction.CountryName} {transaction.LatestRecordedPopulation}"); // Execute query.
            // NotSupportedException: The entity or complex type 'Dixin.Linq.EntityFramework.UniversalProduct' cannot be constructed in a LINQ to Entities query.
        }

        internal static void SelectEntityObjects(WideWorldImporters adventureWorks)
        {
            IQueryable<Country> source = adventureWorks.Countries;
            IEnumerable<Country> transactions = source // Not IQueryable<TransactionHistory>.
                .Where(transaction => transaction is EuropeCountry) // Return IQueryable<Product>. LINQ to Entities.
                .AsEnumerable() // Return IEnumerable<(int, string)>. LINQ to Objects from here.
                .Select(transaction => new EuropeCountry()
                {
                    CountryID = transaction.CountryID,
                    CountryName = transaction.CountryName,
                    LatestRecordedPopulation = transaction.LatestRecordedPopulation,
                }); // Define query.
            transactions.WriteLines(transaction => $"{transaction.GetType().Name} {transaction.CountryName} {transaction.LatestRecordedPopulation}"); // Execute query.
        }

        #endregion

        #region Element

        internal static void First(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            string first = source
                .Select(product => product.StockItemName)
                .First() // Execute query.
                .WriteLine();
        }

        internal static void FirstOrDefault(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            var firstOrDefault = source
                .Select(product => new { Name = product.StockItemName, ListPrice = product.UnitPrice })
                .FirstOrDefault(product => product.ListPrice > 5000); // Execute query.
            firstOrDefault?.Name.WriteLine();
        }

        internal static void Last(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            StockItem last = source.Last(); // Execute query.
#if NETFX
            // NotSupportedException: LINQ to Entities does not recognize the method 'Dixin.Linq.EntityFramework.Product Last[Product](System.Linq.IQueryable`1[Dixin.Linq.EntityFramework.Product])' method, and this method cannot be translated into a store expression.
#endif
            $"{last.StockItemName}: {last.UnitPrice}".WriteLine();
        }

        internal static void LastOrDefault(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            StockItem lastOrDefault = source.LastOrDefault(product => product.UnitPrice < 0); // Execute query.
#if NETFX
            // NotSupportedException: LINQ to Entities does not recognize the method 'Dixin.Linq.EntityFramework.Product LastOrDefault[Product](System.Linq.IQueryable`1[Dixin.Linq.EntityFramework.Product], System.Linq.Expressions.Expression`1[System.Func`2[Dixin.Linq.EntityFramework.Product,System.Boolean]])' method, and this method cannot be translated into a store expression.
#endif
            (lastOrDefault == null).WriteLine(); // True
        }

        internal static void Single(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            var single = source
                .Select(product => new { Name = product.StockItemName, ListPrice = product.UnitPrice })
                .Single(product => product.ListPrice > 1000); // Execute query.
            $"{single.Name}: {single.ListPrice}".WriteLine();
        }

        internal static void SingleOrDefault(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            var singleOrDefault = source
                .GroupBy(
                    subcategory => subcategory.UnitPrice,
                    (key, group) => new { ListPrice = key, Count = group.Count() })
                .SingleOrDefault(group => group.Count > 40); // Define query.
            singleOrDefault?.ListPrice.WriteLine();
        }

        #endregion

        #region Aggregate

        internal static void Count(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories;
            int count = source.Count().WriteLine(); // Execute query.
        }

        internal static void LongCount(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            long longCount = source.LongCount(product => product.UnitPrice > 0).WriteLine(); // Execute query.
        }

        internal static void Max(WideWorldImporters adventureWorks)
        {
            IQueryable<StockGroup> source = adventureWorks.StockGroups;
            //DateTime max = source.Select(photo => photo.ValidTo).Max().WriteLine(); // Execute query.
        }

        internal static void Min(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            decimal min = source.Min(product => product.UnitPrice).WriteLine(); // Execute query.
        }

        internal static void Average(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            decimal average = source.Select(product => product.UnitPrice).Average().WriteLine(); // Execute query.
        }

        internal static void Sum(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            decimal average = source.Sum(product => product.UnitPrice).WriteLine(); // Execute query.
        }

        #endregion

        #region Quantifiers

        internal static void Any(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            bool anyUniversal = source.Any().WriteLine(); // Execute query.
        }

        internal static void Contains(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            // Only primitive types or enumeration types are supported.
            bool contains = source.Select(product => product.UnitPrice).Contains(100).WriteLine(); // Execute query.
        }

        internal static void AnyWithPredicate(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            bool anyUniversal = source.Any(product => product.UnitPrice == 100).WriteLine(); // Execute query.
        }

        internal static void AllNot(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            bool allNot = source.All(product => product.SupplierID != null).WriteLine(); // Execute query.
        }

        internal static void NotAny(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            bool notAny = !source.Any(product => !(product.SupplierID != null)).WriteLine(); // Execute query.
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
            source is IQueryable<TElement> ? (IQueryable<TElement>)source : new EnumerableQuery<TElement>(source);
    }
}
#endif
