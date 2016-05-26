namespace Dixin.Linq.EntityFramework
{
    using System.Collections.Generic;
    using System.Data.Linq.SqlClient;
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

        internal static void WhereWithLike()
        {
            IQueryable<vProductAndDescription> source = AdventureWorks.ProductAndDescriptions;
            IQueryable<vProductAndDescription> descriptions = source.Where(description => description.CultureID.StartsWith("zh")); // Define query.
            descriptions.ForEach(description => Trace.WriteLine($"{description.Name}: {description.Description}")); // Execute query.
        }

        internal static void WhereWithLikeMethod()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<Product> products = source.Where(product =>
                SqlMethods.Like(product.Name, "%Mountain%")); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
            // NotSupportedException: LINQ to Entities does not recognize the method 'Boolean Like(System.String, System.String)' method, and this method cannot be translated into a store expression.
        }

        internal static void WhereWithContains()
        {
            string[] names = { "Blade", "Chainring", "Freewheel" };
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<Product> products = source.Where(product => names.Contains(product.Name)); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        internal static void WhereWithNull()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<Product> products = source.Where(product => product.ProductSubcategory != null); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        internal static void WhereWithStringIsNullOrEmpty()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<Product> products = source.Where(product => string.IsNullOrEmpty(product.Name)); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
            // NotSupportedException: Method 'Boolean IsNullOrEmpty(System.String)' has no supported translation to SQL.
        }

        internal static void OfType()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<UniversalProduct> products = source.OfType<UniversalProduct>(); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.GetType().Name}")); // Execute query.
        }

        #endregion

        #region Mapping

        internal static void Select()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<string> names = source
                .Where(product => product.ListPrice > 100)
                .Select(product => product.ProductID + ": " + product.Name); // Define query.
            names.ForEach(name => Trace.WriteLine(name)); // Execute query.
        }

        internal static void SelectWithStringConcat()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<string> names = source
                .Where(product => product.ListPrice > 100)
                .Select(product => string.Concat(product.ProductID, ": ", product.Name)); // Define query.
            names.ForEach(name => Trace.WriteLine(name)); // Execute query.
        }

        internal static void SelectAnonymousType()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var products = source
                .Where(product => product.ListPrice > 100)
                .Select(product => new { Id = product.ProductID, Name = product.Name }); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Id}: {product.Name}")); // Execute query.
        }

        internal static void SelectEntity()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<Product> products = source
                .Where(product => product.ListPrice > 100)
                .Select(product => new Product() { ProductID = product.ProductID, Name = product.Name }); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.ProductID}: {product.Name}")); // Execute query.
            // NotSupportedException: The entity or complex type 'Dixin.Linq.EntityFramework.Product' cannot be constructed in a LINQ to Entities query.
        }

        internal static void SelectEntityObjects()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IEnumerable<Product> products = source
                .Where(product => product.ListPrice > 100)
                .Select(product => product.Name) // IQueryable<string>, LINQ to SQL.
                .AsEnumerable() // IEnumerable<string>, LINQ to Objects.
                .Select(name => new Product() { Name = name }); // Define query.
            products.ForEach(product => Trace.WriteLine(product.Name)); // Execute query.
        }

        internal static void SelectWithCase()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var products = source.Select(product =>
                new { Name = product.Name, HasListPrice = product.ListPrice > 0 }); // Define query.
            products.ForEach(product => Trace.WriteLine(
                $"{product.Name} has{(product.HasListPrice ? null : " no")} list price.")); // Execute query.
        }

        #endregion

        #region Grouping

        internal static void GroupBy()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            IQueryable<IGrouping<int, string>> groups = source.GroupBy(
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
                (key, values) => new { Key = key, Count = values.Count() }); // Define query.
            groups.ForEach(group => Trace.WriteLine($"{group.Key}: {group.Count}")); // Execute query.
        }

        internal static void GroupByAndSelect()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            var groups = source
                .GroupBy(
                    subcategory => subcategory.ProductCategoryID,
                    subcategory => subcategory.Name)
                .Select(group => new { Key = group.Key, Count = group.Count() }); // Define query.
            groups.ForEach(group => Trace.WriteLine($"{group.Key}: {group.Count}")); // Execute query.
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

        internal static void InnerJoinWithAssociation()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            var subcategories = outer.Select(subcategory =>
                new { Subcategory = subcategory.Name, Category = subcategory.ProductCategory.Name }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Category}: {subcategory.Subcategory}")); // Execute query.
        }

        internal static void MultipleInnerJoinsWithAssociations()
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

        internal static void InnerJoinWithGroupJoin()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> inner = AdventureWorks.ProductCategories;
            var subcategories = outer
                .GroupJoin(
                    inner,
                    subcategory => subcategory.ProductCategoryID,
                    category => category.ProductCategoryID,
                    (subcategory, categories) => new
                    {
                        Subcategory = subcategory.Name,
                        Categories = categories
                    })
                .SelectMany(
                    subcategory => subcategory.Categories,
                    (subcategory, category) =>
                        new { Subcategory = subcategory.Subcategory, Category = category.Name }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Category}: {subcategory.Subcategory}")); // Execute query.
        }

        internal static void InnerJoinWithMultipleKeys()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> inner = AdventureWorks.ProductCategories;
            var subcategories = outer.Join(
                inner,
                subcategory =>
                    new { Id = subcategory.ProductCategoryID, FirstLetter = subcategory.Name.Substring(0, 1) },
                category =>
                    new { Id = category.ProductCategoryID, FirstLetter = category.Name.Substring(0, 1) },
                (subcategory, category) => new { Subcategory = subcategory.Name, Category = category.Name }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine($"{subcategory.Category}: {subcategory.Subcategory}")); // Execute query.
        }

        internal static void LeftOuterJoinWithGroupJoin()
        {
            IQueryable<ProductCategory> outer = AdventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = AdventureWorks.ProductSubcategories;
            var categories = outer
                .GroupJoin(
                    inner,
                    category => category.ProductCategoryID,
                    subcategory => subcategory.ProductCategoryID,
                    (category, subcategories) => new
                    {
                        Category = category.Name,
                        Subcategories = subcategories.Select(subcategory => subcategory.Name)
                    }); // Define query.
            categories.ForEach(category => Trace.WriteLine(
                $"{category.Category} <- {string.Join(", ", category.Subcategories)}")); // Execute query.
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
                    (category => category.Subcategories.DefaultIfEmpty(),
                    (category, subcategory) =>
                        new { Category = category.Category.Name, Subcategory = subcategory.Name }); // Define query.
            categories.ForEach(category => Trace.WriteLine(
                $"{category.Category} <- {category.Subcategory}")); // Execute query.
        }

        internal static void LeftOuterJoinWithSelect()
        {
            IQueryable<ProductCategory> outer = AdventureWorks.ProductCategories;
            IQueryable<ProductSubcategory> inner = AdventureWorks.ProductSubcategories;
            var categories = outer
                .Select(category => new
                {
                    Category = category,
                    Subategories = inner
                            .Where(subcategory => subcategory.ProductCategoryID == category.ProductCategoryID)
                })
                .SelectMany(
                    category => category.Subategories.DefaultIfEmpty(),
                    (category, subcategory) =>
                        new { Category = category.Category.Name, Subcategory = subcategory.Name }); // Define query.
            categories.ForEach(category => Trace.WriteLine(
                $"{category.Category} <- {category.Subcategory}")); // Execute query.
        }

        internal static void LeftOuterJoinWithAssociation()
        {
            IQueryable<ProductCategory> source = AdventureWorks.ProductCategories;
            var categories = source.SelectMany(
                category => category.ProductSubcategories.DefaultIfEmpty(), // INNER JOIN if DefaultIfEmpty is missing.
                (category, subcategory) =>
                    new { Category = category.Name, Subcategory = subcategory.Name }); // Define query.
            categories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Category} <- {subcategory.Subcategory}")); // Execute query.
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
            var products = outer
                .GroupJoin(
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

        #region Concatenation

        internal static void Concat()
        {
            IQueryable<string> first = AdventureWorks.Products
                .Where(product => product.ListPrice < 100)
                .Select(product => product.Name);
            IQueryable<string> second = AdventureWorks.Products
                .Where(product => product.ListPrice > 500)
                .Select(product => product.Name);
            IQueryable<string> concat = first.Concat(second); // Define query.
            concat.ForEach(product => Trace.WriteLine(product)); // Execute query.
        }

        internal static void ConcatWithSelect()
        {
            IQueryable<Product> first = AdventureWorks.Products.Where(product => product.ListPrice < 100);
            IQueryable<Product> second = AdventureWorks.Products.Where(product => product.ListPrice > 500);
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
            IQueryable<int> categories = source.Select(subcategory => subcategory.ProductCategoryID).Distinct(); // Define query.
            categories.ForEach(category => Trace.WriteLine(category)); // Execute query.
        }

        internal static void DistinctWithGroupBy()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            IQueryable<int> distinct = source.GroupBy(
                subcategory => subcategory.ProductCategoryID,
                (key, group) => key); // Define query.
            distinct.ForEach(category => Trace.WriteLine(category)); // Execute query.
        }

        internal static void DistinctWithGroupByAndFirstOrDefault()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            IQueryable<ProductSubcategory> distinct = source.GroupBy(
                subcategory => subcategory.ProductCategoryID,
                (key, group) => group.FirstOrDefault()); // Define query.
            distinct.ForEach(subcategory => Trace.WriteLine(subcategory.Name)); // Execute query.
        }

        internal static void DistinctWithGroupByAndTake()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            IQueryable<ProductSubcategory> distinct = source
                .GroupBy(subcategory => subcategory.ProductCategoryID)
                .SelectMany(group => group.Take(1)); // Define query.
            distinct.ForEach(subcategory => Trace.WriteLine(subcategory.Name)); // Execute query.
        }

        internal static void DistinctWithGroupByAndSelectAndFirstOrDefault()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            IQueryable<string> distinct = source.GroupBy(
                    subcategory => subcategory.ProductCategoryID,
                    (key, group) => group.Select(subcategory => subcategory.Name).FirstOrDefault()); // Define query.
            distinct.ForEach(category => Trace.WriteLine(category)); // Execute query.
        }

        internal static void DistinctWithGroupByAndSelectAndTake()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            IQueryable<string> distinct = source
                .GroupBy(subcategory => subcategory.ProductCategoryID)
                .SelectMany(group => group.Select(subcategory => subcategory.Name).Take(1)); // Define query.
            distinct.ForEach(category => Trace.WriteLine(category)); // Execute query.
        }

        internal static void Intersect()
        {
            IQueryable<int> first = AdventureWorks.Products
                .Where(product => product.ListPrice > 100)
                .Select(product => product.ProductID);
            IQueryable<int> second = AdventureWorks.Products
                .Where(product => product.ListPrice < 500)
                .Select(product => product.ProductID);
            IQueryable<int> union = first.Intersect(second); // Define query.
            union.ForEach(product => Trace.WriteLine(product)); // Execute query.
        }

        internal static void Except()
        {
            IQueryable<int> first = AdventureWorks.Products
                .Where(product => product.ListPrice > 100)
                .Select(product => product.ProductID);
            IQueryable<int> second = AdventureWorks.Products
                .Where(product => product.ListPrice > 500)
                .Select(product => product.ProductID);
            IQueryable<int> except = first.Except(second); // Define query.
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
            IQueryable<string> names = source
                .OrderBy(product => product.Name)
                .Skip(10)
                .Select(product => product.Name); // Define query.
            names.ForEach(name => Trace.WriteLine(name)); // Execute query.
        }

        internal static void Take()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<string> names = source
                .Take(10)
                .Select(product => product.Name); // Define query.
            names.ForEach(name => Trace.WriteLine(name)); // Execute query.
        }

        internal static void OrderByAndSkipAndTake()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<string> names = source
                .OrderBy(product => product.Name)
                .Skip(20)
                .Take(10)
                .Select(product => product.Name); // Define query.
            names.ForEach(name => Trace.WriteLine(name)); // Execute query.
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
            // NotSupportedException: LINQ to Entities does not recognize the method 'System.Linq.IQueryable`1[Dixin.Linq.EntityFramework.Product] Reverse[Product](System.Linq.IQueryable`1[Dixin.Linq.EntityFramework.Product])' method, and this method cannot be translated into a store expression.
        }

        #endregion

        #region Conversion

        internal static void Cast()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<UniversalProduct> universalProducts = source
                .Where(product => product.Name.StartsWith("Road-750"))
                .Cast<UniversalProduct>(); // Define query.
            universalProducts.ForEach(product => Trace.WriteLine($"{product.Name}: {product.GetType().Name}")); // Execute query.
            // NotSupportedException: Unable to cast the type 'Dixin.Linq.EntityFramework.Product' to type 'Dixin.Linq.EntityFramework.UniversalProduct'. LINQ to Entities only supports casting EDM primitive or enumeration types.
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
            var first = source
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                .FirstOrDefault(product => product.ListPrice == 1); // Execute query.
            Trace.WriteLine($"{first?.Name}");
        }

        internal static void Last()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            Product first = source.Last(); // Execute query.
            // NotSupportedException: LINQ to Entities does not recognize the method 'Dixin.Linq.EntityFramework.Product Last[Product](System.Linq.IQueryable`1[Dixin.Linq.EntityFramework.Product])' method, and this method cannot be translated into a store expression.
            Trace.WriteLine($"{first.Name}: {first.ListPrice}");
        }

        internal static void LastOrDefault()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            Product first = source.LastOrDefault(product => product.ListPrice < 0); // Execute query.
            // NotSupportedException: LINQ to Entities does not recognize the method 'Dixin.Linq.EntityFramework.Product LastOrDefault[Product](System.Linq.IQueryable`1[Dixin.Linq.EntityFramework.Product], System.Linq.Expressions.Expression`1[System.Func`2[Dixin.Linq.EntityFramework.Product,System.Boolean]])' method, and this method cannot be translated into a store expression.
            Trace.WriteLine($"{first.Name}: {first.ListPrice}");
        }

        internal static void Single()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var single = source
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                .Single(product => product.Name == "Road-750 Black, 52"); // Execute query.
            Trace.WriteLine($"{single.Name}: {single.ListPrice}");
        }

        internal static void SingleOrDefault()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var singleOrDefault = source
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                .SingleOrDefault(product => product.ListPrice == 540M); // Execute query.
            Trace.WriteLine($"{singleOrDefault?.Name}");
        }

        #endregion

        #region Aggregate

        internal static void Count()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            int count = source.Count(product => product.ListPrice == 0); // Execute query.
            Trace.WriteLine(count);
        }

        internal static void LongCount()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            long longCount = source.LongCount(); // Execute query.
            Trace.WriteLine(longCount);
        }

        internal static void Min()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            decimal min = source.Select(product => product.ListPrice).Where(price => price > 0).Min(); // Execute query.
            Trace.WriteLine(min);
        }

        internal static void Max()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            decimal max = source.OfType<UniversalProduct>().Max(product => product.ListPrice); // Execute query.
            Trace.WriteLine(max);
        }

        internal static void Sum()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            decimal average = source.Sum(product => product.ListPrice); // Execute query.
            Trace.WriteLine(average);
        }

        internal static void Average()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            decimal average = source.Select(product => product.ListPrice).Where(price => price > 0).Average(); // Execute query.
            Trace.WriteLine(average);
        }

        #endregion

        #region Quantifiers

        internal static void All()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            bool allPricePositive = source.All(product => product.ListPrice > 0); // Execute query.
            Trace.WriteLine(allPricePositive);
        }

        internal static void Any()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            bool allPricePositive = source.Any(); // Execute query.
            Trace.WriteLine(allPricePositive);
        }

        internal static void Contains()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            bool contains = source.Select(product => product.ListPrice).Contains(9.99M); // Execute query.
            Trace.WriteLine(contains);
        }

        #endregion
    }
}
