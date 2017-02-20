namespace Tutorial.LinqToSql
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    internal static partial class QueryMethods
    {
        private static readonly AdventureWorks AdventureWorks = new AdventureWorks();
    }

    internal static partial class QueryMethods
    {
        #region Generation

        internal static void DefaultIfEmpty()
        {
            IQueryable<ProductCategory> source = AdventureWorks.ProductCategories;
            IQueryable<ProductCategory> categories = source.DefaultIfEmpty(); // Define query.
            categories.ForEach(category => Trace.WriteLine(category?.Name)); // Execute query.
        }

        internal static void DefaultIfEmptyWithPrimitive()
        {
            IQueryable<ProductCategory> source = AdventureWorks.ProductCategories;
            IQueryable<int> categories = source
                .Select(category => category.ProductCategoryID)
                .DefaultIfEmpty(-1); // Define query.
            categories.ForEach(category => Trace.WriteLine(category)); // Execute query.
        }

        internal static void DefaultIfEmptyWithEntity()
        {
            ProductCategory defaultCategory = new ProductCategory();
            IQueryable<ProductCategory> source = AdventureWorks.ProductCategories;
            IQueryable<ProductCategory> categories = source.DefaultIfEmpty(defaultCategory); // Define query.
            categories.ForEach(category => Trace.WriteLine(category?.Name)); // Execute query.
            // NotSupportedException: Unable to create a constant value of type 'Tutorial.EntityFramework.ProductCategory'. Only primitive types or enumeration types are supported in this context.
        }

        #endregion

        #region Filtering

        internal static void Where()
        {
            IQueryable<ProductCategory> source = AdventureWorks.ProductCategories;
            IQueryable<ProductCategory> categories = source.Where(category => category.ProductCategoryID > 0); // Define query.
            categories.ForEach(category => Trace.WriteLine(category.Name)); // Execute query.
        }

        internal static void WhereWithOr()
        {
            IQueryable<ProductCategory> source = AdventureWorks.ProductCategories;
            IQueryable<ProductCategory> categories = source.Where(category =>
                category.ProductCategoryID <= 1 || category.ProductCategoryID >= 4); // Define query.
            categories.ForEach(category => Trace.WriteLine(category.Name)); // Execute query.
        }

        internal static void WhereWithAnd()
        {
            IQueryable<ProductCategory> source = AdventureWorks.ProductCategories;
            IQueryable<ProductCategory> categories = source.Where(category =>
                category.ProductCategoryID > 0 && category.ProductCategoryID < 5); // Define query.
            categories.ForEach(category => Trace.WriteLine(category.Name)); // Execute query.
        }

        internal static void WhereAndWhere()
        {
            IQueryable<ProductCategory> source = AdventureWorks.ProductCategories;
            IQueryable<ProductCategory> categories = source
                .Where(category => category.ProductCategoryID > 0)
                .Where(category => category.ProductCategoryID < 5); // Define query.
            categories.ForEach(category => Trace.WriteLine(category.Name)); // Execute query.
        }

        internal static void WhereWithIs()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<Product> products = source.Where(product => product is UniversalProduct); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.GetType().Name}")); // Execute query.
        }

        internal static void OfTypeWithEntity()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<UniversalProduct> products = source.OfType<UniversalProduct>(); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.GetType().Name}")); // Execute query.
        }

        internal static void OfTypeWithPrimitive()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<int> products = source.Select(p => p.ProductID).OfType<int>(); // Define query.
            products.ForEach(product => Trace.WriteLine(product)); // Execute query.
            // NotSupportedException: 'System.Int32' is not a valid metadata type for type filtering operations. Type filtering is only valid on entity types and complex types.
        }

        #endregion

        #region Mapping

        internal static void Select()
        {
            IQueryable<ProductCategory> source = AdventureWorks.ProductCategories;
            IQueryable<string> categories = source.Select(category =>
                category.Name + category.Name); // Define query.
            categories.ForEach(category => Trace.WriteLine(category)); // Execute query.
        }

        internal static void SelectWithStringConcat()
        {
            IQueryable<ProductCategory> source = AdventureWorks.ProductCategories;
            IQueryable<string> categories = source.Select(category =>
                string.Concat(category.Name, category.Name)); // Define query.
            categories.ForEach(category => Trace.WriteLine(category)); // Execute query.
        }

        internal static void SelectAnonymousType()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var products = source.Select(product =>
                new { Name = product.Name, IsExpensive = product.ListPrice > 1000, Constant = 1 }); // Define query.
            products.ForEach(product => Trace.WriteLine(product.Name)); // Execute query.
        }

        #endregion

        #region Grouping

        internal static void GroupBy()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            IQueryable<IGrouping<int?, string>> groups = source.GroupBy(
                subcategory => subcategory.ProductCategoryID,
                subcategory => subcategory.Name); // Define query.
            groups.ForEach(group => Trace.WriteLine($"{group.Key}: {string.Join(", ", group)}")); // Execute query.
        }

        internal static void GroupByWithResultSelector()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            var groups = source.GroupBy(
                subcategory => subcategory.ProductCategoryID,
                subcategory => subcategory.Name,
                (key, group) => new { CategoryID = key, SubcategoryCount = group.Count() }); // Define query.
            groups.ForEach(group => Trace.WriteLine($"{group.CategoryID}: {group.SubcategoryCount}")); // Execute query.
        }

        internal static void GroupByAndSelect()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            var groups = source
                .GroupBy(
                    subcategory => subcategory.ProductCategoryID,
                    subcategory => subcategory.Name)
                .Select(group => new { CategoryID = group.Key, SubcategoryCount = group.Count() }); // Define query.
            groups.ForEach(group => Trace.WriteLine($"{group.CategoryID}: {group.SubcategoryCount}")); // Execute query.
        }

        internal static void GroupByAndSelectMany()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            IQueryable<ProductSubcategory> distinct = source
                .GroupBy(subcategory => subcategory.ProductCategoryID)
                .SelectMany(group => group); // Define query.
            distinct.ForEach(subcategory => Trace.WriteLine(subcategory.Name)); // Execute query.
        }

        internal static void GroupByMultipleKeys()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var groups = source.GroupBy(
                product => new { ProductSubcategoryID = product.ProductSubcategoryID, ListPrice = product.ListPrice },
                (key, group) => new
                {
                    ProductSubcategoryID = key.ProductSubcategoryID,
                    ListPrice = key.ListPrice,
                    Count = group.Count()
                }); // Define query.
            groups.ForEach(group => Trace.WriteLine(
                $"{group.ProductSubcategoryID}, {group.ListPrice}: {group.Count}")); // Execute query.
        }

        #endregion

        #region Join

        internal static void InnerJoinWithJoin()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> inner = AdventureWorks.ProductCategories;
            var subcategories = outer.Join(
                inner,
                subcategory => subcategory.ProductCategoryID,
                category => category.ProductCategoryID,
                (subcategory, category) => new { Subcategory = subcategory.Name, Category = category.Name }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Category}: {subcategory.Subcategory}")); // Execute query.
        }

        internal static void InnerJoinWithSelectMany()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> inner = AdventureWorks.ProductCategories;
            var subcategories = outer
                .SelectMany(
                    subcategory => inner,
                    (subcategory, category) => new { Subcategory = subcategory, Category = category })
                .Where(crossJoinValue =>
                    crossJoinValue.Subcategory.ProductCategoryID == crossJoinValue.Category.ProductCategoryID)
                .Select(crossJoinValue =>
                    new { Subcategory = crossJoinValue.Subcategory.Name, Category = crossJoinValue.Category.Name }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Category}: {subcategory.Subcategory}")); // Execute query.
        }

        internal static void InnerJoinWithGroupJoin()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> inner = AdventureWorks.ProductCategories;
            var subcategories = outer
                .GroupJoin(
                    inner,
                    subcategory => subcategory.ProductCategoryID,
                    category => category.ProductCategoryID,
                    (subcategory, categories) => new { Subcategory = subcategory, Categories = categories })
                .SelectMany(
                    subcategory => subcategory.Categories, // LEFT OUTER JOIN if DefaultIfEmpty is called.
                    (subcategory, category) =>
                        new { Subcategory = subcategory.Subcategory.Name, Category = category.Name }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Category}: {subcategory.Subcategory}")); // Execute query.
        }

        internal static void InnerJoinWithSelect()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> inner = AdventureWorks.ProductCategories;
            var categories = outer
                .Select(subcategory => new
                {
                    Subcategory = subcategory,
                    Categories = inner.Where(category => category.ProductCategoryID == subcategory.ProductCategoryID)
                })
                .SelectMany(
                    subcategory => subcategory.Categories, // LEFT OUTER JOIN if DefaultIfEmpty is called.
                    (subcategory, category) =>
                        new { Subcategory = subcategory.Subcategory.Name, Category = category.Name }); // Define query.
            categories.ForEach(category => Trace.WriteLine(
                $"{category.Category}: {category.Subcategory}")); // Execute query.
        }

        internal static void InnerJoinWithRelationship()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            var subcategories = outer.Select(subcategory =>
                new { Subcategory = subcategory.Name, Category = subcategory.ProductCategory.Name }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Category}: {subcategory.Subcategory}")); // Execute query.
        }

        internal static void MultipleInnerJoinsWithRelationship()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var products = source.SelectMany(
                product => product.ProductProductPhotos,
                (product, productProductPhoto) => new
                {
                    Product = product.Name,
                    Photo = productProductPhoto.ProductPhoto.LargePhotoFileName
                }); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Product}: {product.Photo}")); // Execute query.
        }

        internal static void InnerJoinWithMultipleKeys()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> inner = AdventureWorks.ProductCategories;
            var subcategories = outer.Join(
                inner,
                subcategory =>
                    new { ProductCategoryID = subcategory.ProductCategoryID.Value, Name = subcategory.Name },
                category =>
                    new { ProductCategoryID = category.ProductCategoryID, Name = category.Name },
                (subcategory, category) => new { Subcategory = subcategory.Name, Category = category.Name }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine($"{subcategory.Category}: {subcategory.Subcategory}")); // Execute query.
        }

        internal static void LeftOuterJoinWithGroupJoin()
        {
            IQueryable<ProductCategory> outer = AdventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = AdventureWorks.ProductSubcategories;
            var categories = outer.GroupJoin(
                inner,
                category => category.ProductCategoryID,
                subcategory => subcategory.ProductCategoryID,
                (category, subcategories) => new
                {
                    Subcategory = category.Name,
                    Products = subcategories.Select(subcategory => subcategory.Name)
                }); // Define query.
            categories.ForEach(category => Trace.WriteLine(
                $"{category.Subcategory}: {string.Join(", ", category.Products)}")); // Execute query.
        }

        internal static void LeftOuterJoinWithSelect()
        {
            IQueryable<ProductCategory> outer = AdventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = AdventureWorks.ProductSubcategories;
            var categories = outer
                .Select(category => new
                {
                    Category = category.Name,
                    Subcategories = inner
                        .Where(subcategory => subcategory.ProductCategoryID == category.ProductCategoryID)
                        .Select(subcategory => subcategory.Name)
                }); // Define query.
            categories.ForEach(category => Trace.WriteLine(
                $"{category.Category}: {string.Join(", ", category.Subcategories)}")); // Execute query.
        }

        internal static void LeftOuterJoinWithGroupJoinAndSelectMany()
        {
            IQueryable<ProductCategory> outer = AdventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = AdventureWorks.ProductSubcategories;
            var categories = outer
                .GroupJoin(
                    inner,
                    category => category.ProductCategoryID,
                    subcategory => subcategory.ProductCategoryID,
                    (category, subcategories) => new { Category = category, Subcategories = subcategories })
                .SelectMany
                    (category => category.Subcategories.DefaultIfEmpty(), // INNER JOIN if DefaultIfEmpty is missing.
                    (category, subcategory) =>
                        new { Category = category.Category.Name, Subcategory = subcategory.Name }); // Define query.
            categories.ForEach(category => Trace.WriteLine(
                $"{category.Category}: {category.Subcategory}")); // Execute query.
        }

        internal static void LeftOuterJoinWithSelectAndSelectMany()
        {
            IQueryable<ProductCategory> outer = AdventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = AdventureWorks.ProductSubcategories;
            var categories = outer
                .Select(category => new
                {
                    Category = category,
                    Subcategories = inner
                        .Where(subcategory => subcategory.ProductCategoryID == category.ProductCategoryID)
                })
                .SelectMany(
                    category => category.Subcategories.DefaultIfEmpty(), // INNER JOIN if DefaultIfEmpty is missing.
                    (category, subcategory) =>
                        new { Category = category.Category.Name, Subcategory = subcategory.Name }); // Define query.
            categories.ForEach(category => Trace.WriteLine(
                $"{category.Category}: {category.Subcategory}")); // Execute query.
        }

        internal static void LeftOuterJoinWithRelationship()
        {
            IQueryable<ProductCategory> source = AdventureWorks.ProductCategories;
            var categories = source.SelectMany(
                category => category.ProductSubcategories.DefaultIfEmpty(), // INNER JOIN if DefaultIfEmpty is missing.
                (category, subcategory) =>
                    new { Category = category.Name, Subcategory = subcategory.Name }); // Define query.
            categories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Category}: {subcategory.Subcategory}")); // Execute query.
        }

        internal static void CrossJoinWithSelectMany()
        {
            IQueryable<Product> outer = AdventureWorks.Products.Where(product => product.ListPrice > 2000);
            IQueryable<Product> inner = AdventureWorks.Products.Where(product => product.ListPrice < 100);
            var bundles = outer.SelectMany(
                outerProduct => inner,
                (outerProduct, innerProduct) =>
                    new { Expensive = outerProduct.Name, Cheap = innerProduct.Name }); // Define query.
            bundles.ForEach(bundle => Trace.WriteLine($"{bundle.Expensive}: {bundle.Cheap}")); // Execute query.
        }

        internal static void CrossJoinWithJoin()
        {
            IQueryable<Product> outer = AdventureWorks.Products.Where(product => product.ListPrice > 2000);
            IQueryable<Product> inner = AdventureWorks.Products.Where(product => product.ListPrice < 100);
            var bundles = outer.Join(
                inner,
                product => true,
                product => true,
                (outerProduct, innerProduct) =>
                    new { Expensive = outerProduct.Name, Cheap = innerProduct.Name }); // Define query.
            bundles.ForEach(bundle => Trace.WriteLine($"{bundle.Expensive}: {bundle.Cheap}")); // Execute query.
        }

        internal static void SelfJoin()
        {
            IQueryable<Product> outer = AdventureWorks.Products;
            IQueryable<Product> inner = AdventureWorks.Products;
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
            products.ForEach(product => Trace.WriteLine(
                $"{product.Name} ({product.ListPrice}): {string.Join(", ", product.SamePriceProducts)}")); // Execute query.
        }

        #endregion

        #region Apply

        internal static void CrossApplyWithGroupByAndTake()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            var categories = source
                .GroupBy(subcategory => subcategory.ProductCategoryID)
                .SelectMany(
                    group => group.Take(1),
                    (group, subcategory) =>
                        new { ProductCategoryID = group.Key, FirstSubcategory = subcategory }); // Define query.
            categories.ForEach(category => Trace.WriteLine(
                $"{category.ProductCategoryID}: {category.FirstSubcategory?.Name}")); // Execute query.
        }

        internal static void CrossApplyWithGroupJoinAndTake()
        {
            IQueryable<ProductCategory> outer = AdventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = AdventureWorks.ProductSubcategories;
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
            categories.ForEach(category => Trace.WriteLine(
                $"{category.Category.Name}: {category.FirstSubcategory?.Name}")); // Execute query.
        }

        internal static void CrossApplyWithRelationshipAndTake()
        {
            IQueryable<ProductCategory> source = AdventureWorks.ProductCategories;
            var categories = source
                .Select(category => new { Category = category, Subcategories = category.ProductSubcategories })
                .SelectMany(
                    category => category.Subcategories.Take(1),
                    (category, subcategory) =>
                        new { Category = category.Category, FirstSubcategory = subcategory }); // Define query.
            categories.ForEach(category => Trace.WriteLine(
                $"{category.Category.Name}: {category.FirstSubcategory?.Name}")); // Execute query.
        }

        internal static void OuterApplyWithGroupByAndFirstOrDefault()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            var categories = source.GroupBy(
                subcategory => subcategory.ProductCategoryID,
                (key, group) =>
                    new { ProductCategoryID = key, FirstSubcategory = group.FirstOrDefault() }); // Define query.
            categories.ForEach(category => Trace.WriteLine(
                $"{category.ProductCategoryID}: {category.FirstSubcategory?.Name}")); // Execute query.
        }

        internal static void OuterApplyWithGroupJoinAndFirstOrDefault()
        {
            IQueryable<ProductCategory> outer = AdventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = AdventureWorks.ProductSubcategories;
            var categories = outer.GroupJoin(
                inner,
                category => category.ProductCategoryID,
                subcategory => subcategory.ProductCategoryID,
                (category, subcategories) =>
                    new { Category = category, FirstSubcategory = subcategories.FirstOrDefault() }); // Define query.
            categories.ForEach(category => Trace.WriteLine(
                $"{category.Category.Name}: {category.FirstSubcategory?.Name}")); // Execute query.
        }

        internal static void OuterApplyWithRelationshipAndFirstOrDefault()
        {
            IQueryable<ProductCategory> source = AdventureWorks.ProductCategories;
            var categories = source.Select(category => new
            {
                Category = category,
                FirstSubcategory = category.ProductSubcategories.FirstOrDefault()
            }); // Define query.
            categories.ForEach(category => Trace.WriteLine(
                $"{category.Category.Name}: {category.FirstSubcategory?.Name}")); // Execute query.
        }

        #endregion

        #region Concatenation

        internal static void Concat()
        {
            IQueryable<string> first = AdventureWorks.Products
                .Where(product => product.ListPrice < 100)
                .Select(product => product.Name);
            IQueryable<string> second = AdventureWorks.Products
                .Where(product => product.ListPrice > 2000)
                .Select(product => product.Name);
            IQueryable<string> concat = first.Concat(second); // Define query.
            concat.ForEach(product => Trace.WriteLine(product)); // Execute query.
        }

        internal static void ConcatWithSelect()
        {
            IQueryable<Product> first = AdventureWorks.Products.Where(product => product.ListPrice < 100);
            IQueryable<Product> second = AdventureWorks.Products.Where(product => product.ListPrice > 2000);
            IQueryable<string> concat = first
                .Concat(second)
                .Select(product => product.Name); // Define query.
            concat.ForEach(product => Trace.WriteLine(product)); // Execute query.
        }

        #endregion

        #region Set

        internal static void Distinct()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            IQueryable<int?> distinct = source
                .Select(subcategory => subcategory.ProductCategoryID)
                .Distinct(); // Define query.
            distinct.ForEach(value => Trace.WriteLine(value)); // Execute query.
        }

        internal static void DistinctWithGroupBy()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            IQueryable<int?> distinct = source.GroupBy(
                subcategory => subcategory.ProductCategoryID,
                (key, group) => key); // Define query.
            distinct.ForEach(value => Trace.WriteLine(value)); // Execute query.
        }

        internal static void DistinctMultipleKeys()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            var distinct = source
                .Select(subcategory =>
                    new { ProductCategoryID = subcategory.ProductCategoryID, Name = subcategory.Name })
                .Distinct(); // Define query.
            distinct.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.ProductCategoryID}: {subcategory.Name}")); // Execute query.
        }

        internal static void DistinctMultipleKeysWithGroupBy()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            var distinct = source.GroupBy(
                subcategory => new { ProductCategoryID = subcategory.ProductCategoryID, Name = subcategory.Name },
                (key, group) => key); // Define query.
            distinct.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.ProductCategoryID}: {subcategory.Name}")); // Execute query.
        }

        internal static void DistinctWithGroupByAndSelectAndFirstOrDefault()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            IQueryable<string> distinct = source.GroupBy(
                subcategory => subcategory.ProductCategoryID,
                (key, group) => group.Select(subcategory => subcategory.Name).FirstOrDefault()); // Define query.
            distinct.ForEach(subcategory => Trace.WriteLine(subcategory)); // Execute query.
        }

        internal static void Intersect()
        {
            var first = AdventureWorks.Products
                .Where(product => product.ListPrice > 100)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice });
            var second = AdventureWorks.Products
                .Where(product => product.ListPrice < 2000)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice });
            var intersect = first.Intersect(second); // Define query.
            intersect.ForEach(product => Trace.WriteLine(product)); // Execute query.
        }

        internal static void Except()
        {
            var first = AdventureWorks.Products
                .Where(product => product.ListPrice > 100)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice });
            var second = AdventureWorks.Products
                .Where(product => product.ListPrice > 2000)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice });
            var except = first.Except(second); // Define query.
            except.ForEach(product => Trace.WriteLine(product)); // Execute query.
        }

        #endregion

        #region Partitioning

        internal static void Skip()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<string> names = source
                .Skip(10)
                .Select(product => product.Name); // Define query.
            names.ForEach(name => Trace.WriteLine(name)); // Execute query.
            // NotSupportedException: The method 'Skip' is only supported for sorted input in LINQ to Entities. The method 'OrderBy' must be called before the method 'Skip'.
        }

        internal static void OrderByAndSkip()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<string> products = source
                .OrderBy(product => product.Name)
                .Skip(10)
                .Select(product => product.Name); // Define query.
            products.ForEach(product => Trace.WriteLine(product)); // Execute query.
        }

        internal static void Take()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<string> products = source
                .Take(10)
                .Select(product => product.Name); // Define query.
            products.ForEach(product => Trace.WriteLine(product)); // Execute query.
        }

        internal static void OrderByAndSkipAndTake()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<string> products = source
                .OrderBy(product => product.Name)
                .Skip(20)
                .Take(10)
                .Select(product => product.Name); // Define query.
            products.ForEach(product => Trace.WriteLine(product)); // Execute query.
        }

        #endregion

        #region Ordering

        internal static void OrderBy()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var products = source
                .OrderBy(product => product.ListPrice)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        internal static void OrderByDescending()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var products = source
                .OrderByDescending(product => product.ListPrice)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        internal static void OrderByAndThenBy()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var products = source
                .OrderBy(product => product.ListPrice)
                .ThenBy(product => product.Name)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        internal static void OrderByAnonymousType()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var products = source
                .OrderBy(product => new { ListPrice = product.ListPrice, Name = product.Name })
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        internal static void OrderByAndOrderBy()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var products = source
                .OrderBy(product => product.ListPrice)
                .OrderBy(product => product.Name)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        internal static void Reverse()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var products = source
                .OrderBy(product => product.ListPrice)
                .Reverse()
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
            // NotSupportedException: LINQ to Entities does not recognize the method 'System.Linq.IQueryable`1[Tutorial.EntityFramework.Product] Reverse[Product](System.Linq.IQueryable`1[Tutorial.EntityFramework.Product])' method, and this method cannot be translated into a store expression.
        }

        #endregion

        #region Conversion

        internal static void CastPrimitive()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<string> listPrices = source
                .Select(product => product.ListPrice)
                .Cast<string>(); // Define query.
            listPrices.ForEach(listPrice => Trace.WriteLine(listPrice)); // Execute query.
        }

        internal static void CastEntity()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<UniversalProduct> universalProducts = source
                .Where(product => product.Name.StartsWith("Road-750"))
                .Cast<UniversalProduct>(); // Define query.
            universalProducts.ForEach(product => Trace.WriteLine($"{product.Name}: {product.GetType().Name}")); // Execute query.
            // NotSupportedException: Unable to cast the type 'Tutorial.EntityFramework.Product' to type 'Tutorial.EntityFramework.UniversalProduct'. LINQ to Entities only supports casting EDM primitive or enumeration types.
        }

        internal static void AsEnumerableAsQueryable()
        {
            IQueryable<Product> source1 = AdventureWorks.Products;
            var query1 = source1 // DbSet<T> object, derives from DbQuery<T>.
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }) // Return DbQuery<T> object.
                .AsEnumerable() // Do nothing, directly return the input DbQuery<T> object.
                .AsQueryable() // Do nothing, directly return the input DbQuery<T> object.
                .Where(product => product.ListPrice > 0); // Continue LINQ to Entities query.
            query1.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}"));

            IQueryable<Product> source2 = AdventureWorks.Products;
            var query2 = source2 // DbSet<T> object, derives from DbQuery<T>.
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }) // Return DbQuery<T> object.
                .AsEnumerable() // Do nothing, directly return the input DbQuery<T> object.
                .Select(product => product) // Enumerable.Select, returns a generator wrapping the input DbQuery<T> object.
                .AsQueryable() // Return an EnumerableQuery<T> object wrapping the input generator.
                .Where(product => product.ListPrice > 0); // No longer LINQ to Entities query on DbSet<T> or DbQuery<T>.
            query2.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}"));
        }

        internal static void SelectEntities()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<Product> products = source
                .Where(product => product is UniversalProduct)
                .Select(product => new UniversalProduct()
                {
                    ProductID = product.ProductID,
                    Name = product.Name,
                    ListPrice = product.ListPrice,
                    ProductSubcategoryID = product.ProductSubcategoryID
                }); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.ProductID}: {product.Name}")); // Execute query.
            // NotSupportedException: The entity or complex type 'Tutorial.EntityFramework.UniversalProduct' cannot be constructed in a LINQ to Entities query.
        }

        internal static void SelectEntityObjects()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IEnumerable<Product> products = source
                .Where(product => product is UniversalProduct) // Return IQueryable<Product>. LINQ to Entities.
                .AsEnumerable() // Return IEnumerable<(int, string)>. LINQ to Objects from here.
                .Select(product => new UniversalProduct()
                {
                    ProductID = product.ProductID,
                    Name = product.Name,
                    ListPrice = product.ListPrice,
                    ProductSubcategoryID = product.ProductSubcategoryID
                }); // Define query.
            products.ForEach(product => Trace.WriteLine(product.Name)); // Execute query.
        }

        #endregion

        #region Element

        internal static void First()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            string first = source
                .Select(product => product.Name)
                .First(); // Execute query.
            Trace.WriteLine(first);
        }

        internal static void FirstOrDefault()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var firstOrDefault = source
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                .FirstOrDefault(product => product.ListPrice > 5000); // Execute query.
            Trace.WriteLine($"{firstOrDefault?.Name}");
        }

        internal static void Last()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            Product first = source.Last(); // Execute query.
            // NotSupportedException: LINQ to Entities does not recognize the method 'Tutorial.EntityFramework.Product Last[Product](System.Linq.IQueryable`1[Tutorial.EntityFramework.Product])' method, and this method cannot be translated into a store expression.
            Trace.WriteLine($"{first.Name}: {first.ListPrice}");
        }

        internal static void LastOrDefault()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            Product first = source.LastOrDefault(product => product.ListPrice < 0); // Execute query.
            // NotSupportedException: LINQ to Entities does not recognize the method 'Tutorial.EntityFramework.Product LastOrDefault[Product](System.Linq.IQueryable`1[Tutorial.EntityFramework.Product], System.Linq.Expressions.Expression`1[System.Func`2[Tutorial.EntityFramework.Product,System.Boolean]])' method, and this method cannot be translated into a store expression.
            Trace.WriteLine($"{first.Name}: {first.ListPrice}");
        }

        internal static void Single()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var single = source
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                .Single(product => product.ListPrice < 50); // Execute query.
            Trace.WriteLine($"{single.Name}: {single.ListPrice}");
        }

        internal static void SingleOrDefault()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var singleOrDefault = source
                .GroupBy(
                    subcategory => subcategory.ListPrice,
                    (key, group) => new { ListPrice = key, Count = group.Count() })
                .SingleOrDefault(group => group.Count > 10); // Define query.
            Trace.WriteLine($"{singleOrDefault?.ListPrice}");
        }

        #endregion

        #region Aggregate

        internal static void Count()
        {
            IQueryable<ProductCategory> source = AdventureWorks.ProductCategories;
            int count = source.Count(); // Execute query.
            Trace.WriteLine(count);
        }

        internal static void LongCount()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            long longCount = source.LongCount(product => product.ListPrice > 0); // Execute query.
            Trace.WriteLine(longCount);
        }

        internal static void Max()
        {
            IQueryable<ProductPhoto> source = AdventureWorks.ProductPhotos;
            DateTime max = source.Select(photo => photo.ModifiedDate).Max(); // Execute query.
            Trace.WriteLine(max);
        }

        internal static void Min()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            decimal min = source.Min(product => product.ListPrice); // Execute query.
            Trace.WriteLine(min);
        }

        internal static void Average()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            decimal average = source.Select(product => product.ListPrice).Average(); // Execute query.
            Trace.WriteLine(average);
        }

        internal static void Sum()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            decimal average = source.Sum(product => product.ListPrice); // Execute query.
            Trace.WriteLine(average);
        }

        #endregion

        #region Quantifiers

        internal static void Any()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            bool anyUniversal = source.Any(); // Execute query.
            Trace.WriteLine(anyUniversal);
        }

        internal static void Contains()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            // Only primitive types or enumeration types are supported.
            bool contains = source.Select(product => product.ListPrice).Contains(100); // Execute query.
            Trace.WriteLine(contains);
        }

        internal static void AnyWithPredicate()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            bool anyUniversal = source.Any(product => product.ListPrice == 100); // Execute query.
            Trace.WriteLine(anyUniversal);
        }

        internal static void AllNot()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            bool allNot = source.All(product => product.ProductSubcategoryID != null); // Execute query.
            Trace.WriteLine(allNot);
        }

        internal static void NotAny()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            bool notAny = !source.Any(product => !(product.ProductSubcategoryID != null)); // Execute query.
            Trace.WriteLine(notAny);
        }

        #endregion
    }
}