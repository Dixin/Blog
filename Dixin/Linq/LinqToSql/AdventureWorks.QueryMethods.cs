namespace Dixin.Linq.LinqToSql
{
    using System.Collections.Generic;
    using System.Data.Linq.SqlClient;
    using System.Diagnostics;
    using System.Linq;

    internal static partial class QueryMethods
    {
        private static readonly AdventureWorks AdventureWorks = new AdventureWorks();

        #region Generation

        internal static void DefaultIfEmpty()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<Product> products = source.DefaultIfEmpty(); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product?.Name}")); // Execute query.
        }

        #endregion

        #region Filtering

        internal static void Where()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<Product> products = source.Where(product => product.ListPrice > 100); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        internal static void WhereWithOr()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<Product> products = source.Where(product => product.ListPrice < 100 || product.ListPrice > 200); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        internal static void WhereWithAnd()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<Product> products = source.Where(product =>
                product.ListPrice > 100 && product.ListPrice < 200); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        internal static void WhereAndWhere()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<Product> products = source
                .Where(product => product.ListPrice > 100)
                .Where(product => product.ListPrice < 200); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        internal static void WhereWithLike()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<Product> products = source.Where(product => product.Name.StartsWith("ML ")); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        internal static void WhereWithLikeMethod()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<Product> products = source.Where(product =>
                SqlMethods.Like(product.Name, "%Mountain%")); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
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
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.Style}")); // Execute query.
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
            // NotSupportedException: Explicit construction of entity type 'Dixin.Linq.LinqToSql.Product' in query is not allowed.
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

        internal static void Grouping()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<IGrouping<string, string>> productGroups = source.GroupBy(
                product => product.Name.Substring(0, 1),
                product => product.Name); // Define query.
            productGroups.ForEach(group => Trace.WriteLine($"{group.Key}: {string.Join(", ", group)}")); // Execute query.
        }

        internal static void GroupBy()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var productGroups = source.GroupBy(
                product => product.Name.Substring(0, 1),
                (key, group) => new { Key = key, Count = group.Count() }); // Define query.
            productGroups.ForEach(group => Trace.WriteLine($"{group.Key}: {group.Count}")); // Execute query.
        }

        internal static void GroupByWithWhere()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var productGroups = source
                .GroupBy(
                    product => product.Name.Substring(0, 1),
                    (key, group) => new { Key = key, Count = group.Count() })
                .Where(group => group.Count > 0); // Define query.
            productGroups.ForEach(group => Trace.WriteLine($"{group.Key}: {group.Count}")); // Execute query.
        }

        #endregion

        #region Join

        internal static void InnerJoin()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> inner = AdventureWorks.ProductCategories;
            var subcategories = outer.Join(
                inner,
                subcategory => subcategory.ProductCategoryID,
                category => category.ProductCategoryID,
                (subcategory, category) => new { Subcategory = subcategory.Name, Category = category.Name }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine($"{subcategory.Category}: {subcategory.Subcategory}")); // Execute query.
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
            IQueryable<ProductCategory> inner = AdventureWorks.ProductCategories;
            var subcategories = outer.Join(
                inner,
                subcategory => subcategory.ProductCategoryID,
                category => category.ProductCategoryID,
                (subcategory, category) => new { Subcategory = subcategory.Name, Category = category.Name }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine($"{subcategory.Category}: {subcategory.Subcategory}")); // Execute query.
        }

        internal static void InnerJoinWithMultipleKeys()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> inner = AdventureWorks.ProductCategories;
            var subcategories = outer.Join(
                inner,
                subcategory =>
                    new { Id = subcategory.ProductCategoryID ?? -1, FirstLetter = subcategory.Name.Substring(0, 1) },
                category =>
                    new { Id = category.ProductCategoryID, FirstLetter = category.Name.Substring(0, 1) },
                (subcategory, category) => new { Subcategory = subcategory.Name, Category = category.Name }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine($"{subcategory.Category}: {subcategory.Subcategory}")); // Execute query.
        }

        internal static void LeftOuterJoin()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> inner = AdventureWorks.ProductCategories;
            var subcategories = outer.GroupJoin(
                inner,
                subcategory => subcategory.ProductCategoryID,
                category => category.ProductCategoryID,
                (subcategory, categories) => new
                    {
                        Subcategory = subcategory.Name,
                        Categories = categories.Select(category => category.Name)
                    }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Subcategory} <- {string.Join(", ", subcategory.Categories)}")); // Execute query.
        }

        internal static void LeftOuterJoinWithDefaultIfEmpty()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> inner = AdventureWorks.ProductCategories;
            var subcategories = outer
                .GroupJoin(
                    inner,
                    subcategory => subcategory.ProductCategoryID,
                    category => category.ProductCategoryID,
                    (subcategory, categories) => new { Subcategory = subcategory.Name, Categories = categories })
                .SelectMany(
                    subcategory => subcategory.Categories.DefaultIfEmpty(),
                    (subcategory, category) =>
                        new { Subcategory = subcategory.Subcategory, Category = category.Name }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Category} <- {subcategory.Subcategory}")); // Execute query.
        }

        internal static void LeftOuterJoinWithSelect()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> inner = AdventureWorks.ProductCategories;
            var subcategories = outer.Select(subcategory => new
            {
                Subcategory = subcategory.Name,
                Categories = inner
                    .Where(category => subcategory.ProductCategoryID == category.ProductCategoryID)
                    .Select(category => category.Name)
            }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Subcategory} <- {string.Join(", ", subcategory.Categories)}")); // Execute query.
        }

        internal static void LeftOuterJoinWithAssociation()
        {
            IQueryable<ProductSubcategory> source = AdventureWorks.ProductSubcategories;
            var subcategories = source.Select(subcategory => new
                {
                    Subcategory = subcategory.Name,
                    Category = subcategory.ProductCategory.Name
                }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Category} <- {subcategory.Subcategory}")); // Execute query.
        }

        internal static void CrossJoin()
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

        internal static void CrossJoinWithSelectMany()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> inner = AdventureWorks.ProductCategories;
            var subcategories = outer.SelectMany(
                subcategory => inner,
                (subcategory, category) => new { Subcategory = subcategory.Name, Category = category.Name }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Category} <- {subcategory.Subcategory}")); // Execute query.
        }

        internal static void CrossJoinWithJoin()
        {
            IQueryable<ProductSubcategory> outer = AdventureWorks.ProductSubcategories;
            IQueryable<ProductCategory> inner = AdventureWorks.ProductCategories;
            var subcategories = outer.Join(
                inner,
                subcategory => true,
                category => true,
                (subcategory, category) => new { Subcategory = subcategory.Name, Category = category.Name }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Category}: {subcategory.Subcategory}")); // Execute query.
        }

        internal static void SelfJoin()
        {
            IQueryable<Product> outer = AdventureWorks.Products;
            IQueryable<Product> inner = AdventureWorks.Products;
            var subcategories = outer.GroupJoin(
                    inner,
                    product => product.ListPrice,
                    product => product.ListPrice,
                    (product, samePrice) => new { Product = product, SamePrice = samePrice })
                .Where(selfJoinValue => selfJoinValue.Product.ListPrice > 0)
                .Select(selfJoinValue => new
                    {
                        Product = selfJoinValue.Product.Name,
                        ListPrice = selfJoinValue.Product.ListPrice,
                        SamePrice = selfJoinValue.SamePrice
                        .Where(product => product.ProductID != selfJoinValue.Product.ProductID)
                        .Select(product => product.Name)
                    }); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Product}: {string.Join(", ", subcategory.SamePrice)}")); // Execute query.
        }

        #endregion

        #region Concatenation

        internal static void Concat()
        {
            var first = AdventureWorks.Products
                .Where(product => product.ListPrice < 100)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice });
            var second = AdventureWorks.Products
                .Where(product => product.ListPrice > 200)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice });
            var concat = first.Concat(second); // Define query.
            concat.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        internal static void ConcatWithSelect()
        {
            IQueryable<Product> first = AdventureWorks.Products.Where(product => product.ListPrice < 100);
            IQueryable<Product> second = AdventureWorks.Products.Where(product => product.ListPrice > 200);
            var concat = first
                .Concat(second)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }); // Define query.
            concat.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        #endregion

        #region Set

        internal static void Distinct()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<int?> subcategories = source.Select(product => product.ProductSubcategoryID).Distinct(); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine(subcategory)); // Execute query.
        }

        internal static void DistinctWithGroupByAndSelect()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var subcategories = source
                .Select(product => new { Subcategory = product.ProductSubcategoryID, Product = product.Name })
                .GroupBy(product => product.Subcategory)
                .Select(group => group.FirstOrDefault()); // Define query. First works.
            subcategories.ForEach(subcategory => Trace.WriteLine($"{subcategory.Subcategory}: {subcategory.Product}")); // Execute query.
        }

        internal static void DistinctWithGroupByAndSelectMany()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var subcategories = source
                .Select(product => new { Subcategory = product.ProductSubcategoryID, Product = product.Name })
                .GroupBy(product => product.Subcategory)
                .SelectMany(group => group.Take(1)); // Define query.
            subcategories.ForEach(subcategory => Trace.WriteLine($"{subcategory.Subcategory}: {subcategory.Product}")); // Execute query.
        }

        internal static void Intersect()
        {
            IQueryable<Product> first = AdventureWorks.Products.Where(product => product.ListPrice > 100);
            IQueryable<Product> second = AdventureWorks.Products.Where(product => product.ListPrice < 200);
            var union = first
                .Intersect(second)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }); // Define query.
            union.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        internal static void Except()
        {
            IQueryable<Product> first = AdventureWorks.Products.Where(product => product.ListPrice > 100);
            IQueryable<Product> second = AdventureWorks.Products.Where(product => product.ListPrice > 200);
            var union = first
                .Except(second)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }); // Define query.
            union.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        #endregion

        #region Partitioning

        internal static void Skip()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<string> names = source.Skip(10).Select(product => product.Name); // Define query.
            names.ForEach(name => Trace.WriteLine(name)); // Execute query.
        }

        internal static void OrderBySkip()
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
            IQueryable<string> names = source.Take(10).Select(product => product.Name); // Define query.
            names.ForEach(name => Trace.WriteLine(name)); // Execute query.
        }

        internal static void OrderBySkipTake()
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

        internal static void OrderByThenBy()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var products = source
                .OrderBy(product => product.ListPrice)
                .ThenBy(product => product.Name)
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice }); // Define query.
            products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
        }

        internal static void OrderByOrderBy()
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
            universalProducts.ForEach(product => Trace.WriteLine($"{product.Name}: {product.Style}")); // Execute query.
        }

        #endregion

        #region Element

        internal static void First()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            var first = source
                .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                .First(); // Execute query.
            Trace.WriteLine($"{first.Name}: {first.ListPrice}");
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
            // NotSupportedException: The query operator 'Last' is not supported.
            Trace.WriteLine($"{first.Name}: {first.ListPrice}");
        }

        internal static void LastOrDefault()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            Product first = source.LastOrDefault(product => product.ListPrice < 0); // Execute query.
            // NotSupportedException: The query operator 'LastOrDefault' is not supported.
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