namespace Dixin.Linq.LinqToEntities
{
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Linq;

    public static partial class QueryMethods
    {
        public static void EntitiesFromSameContext()
        {
            using (AdventureWorksDbContext adventureWorks = new AdventureWorksDbContext())
            {
                Product productById = adventureWorks.Products.Single(product => product.ProductID == 999);
                Product productByName = adventureWorks.Products.Single(product => product.Name == "Road-750 Black, 52");
                Trace.WriteLine(object.ReferenceEquals(productById, productByName)); // True.
            }
        }

        public static void MappingsFromSameContext()
        {
            using (AdventureWorksDbContext adventureWorks = new AdventureWorksDbContext())
            {
                var productById = adventureWorks.Products
                    .Where(product => product.ProductID == 999)
                    .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                    .Single();
                var productByName = adventureWorks.Products
                    .Where(product => product.Name == "Road-750 Black, 52")
                    .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                    .Single();
                Trace.WriteLine(object.ReferenceEquals(productById, productByName)); // False.
            }
        }

        public static void EntitiesFromContexts()
        {
            Product productById;
            Product productByName;
            using (AdventureWorksDbContext adventureWorks = new AdventureWorksDbContext())
            {
                productById = adventureWorks.Products.Single(product => product.ProductID == 999);
            }
            using (AdventureWorksDbContext adventureWorks = new AdventureWorksDbContext())
            {
                productByName = adventureWorks.Products.Single(product => product.Name == "Road-750 Black, 52");
            }
            Trace.WriteLine(object.ReferenceEquals(productById, productByName)); // False.
        }

        public static void TrackChanges()
        {
            using (AdventureWorksDbContext adventureWorks = new AdventureWorksDbContext())
            {
                IQueryable<Product> update = adventureWorks.Products
                    .Where(product => product.Name.Contains("HL"));
                update.ForEach(product => product.ListPrice += 50);
                IQueryable<Product> delete = adventureWorks.Products
                    .Where(product => product.Name.Contains("ML")).AsNoTracking();
                adventureWorks.Products.RemoveRange(delete);
                Product insert = new Product() { Name = "Insert", ListPrice = 123 };
                adventureWorks.Products.Add(insert);
                Trace.WriteLine(adventureWorks.ChangeTracker.HasChanges()); // True.
                adventureWorks.ChangeTracker.Entries<Product>().ForEach(change =>
                    {
                        Trace.Write($"{change.State}: ");
                        Product product = change.Entity;
                        switch (change.State)
                        {
                            case EntityState.Added:
                            case EntityState.Deleted:
                            case EntityState.Unchanged:
                                Trace.WriteLine($"{product.ProductID}, {product.Name}, {product.ListPrice}");
                                break;
                            case EntityState.Modified:
                                Trace.WriteLine(string.Join(", ", change.CurrentValues.PropertyNames.Select(
                                    property => $"{change.OriginalValues[property]} -> {change.CurrentValues[property]}")));
                                break;
                        }
                    });
                // Added: 0, Insert, 123
                // Modified: 951 -> 951, 8 -> 8, HL Crankset -> HL Crankset, 404.9900 -> 454.9900
                // Modified: 996 -> 996, 5 -> 5, HL Bottom Bracket -> HL Bottom Bracket, 121.4900 -> 171.4900
                // Deleted: 950, ML Crankset, 256.4900
                // Deleted: 995, ML Bottom Bracket, 101.2400
            }
        }

        public static void Attach()
        {
            using (AdventureWorksDbContext adventureWorks = new AdventureWorksDbContext())
            {
                Product product = new Product() { Name = "On the fly", ListPrice = 1 };
                product.ListPrice = 2;
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries<Product>().Any()); // False.
                adventureWorks.Products.Attach(product);
                product.ListPrice = 3;
                adventureWorks.ChangeTracker.Entries<Product>()
                    .Where(change => change.State == EntityState.Modified)
                    .ForEach(change => Trace.WriteLine(
                        $"{change.OriginalValues[nameof(Product.ListPrice)]} -> {change.CurrentValues[nameof(Product.ListPrice)]}")); // 2 -> 3.
            }
        }

        public static void AssociationChanges()
        {
            using (AdventureWorksDbContext adventureWorks = new AdventureWorksDbContext())
            {
                ProductCategory firstCategory = adventureWorks.ProductCategories
                    .Include(category => category.ProductSubcategories).First();
                ProductSubcategory[] subcategories = firstCategory.ProductSubcategories.ToArray();
                Trace.WriteLine(firstCategory.ProductSubcategories.Count); // 12.
                Trace.WriteLine(subcategories.All(subcategory => subcategory.ProductCategory == firstCategory)); // True.

                firstCategory.ProductSubcategories.Clear();
                Trace.WriteLine(firstCategory.ProductSubcategories.Count); // 0.
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries<Product>().Any()); // False.
                Trace.WriteLine(subcategories.All(subcategory => subcategory.ProductCategory == null)); // True.
            }
        }

        public static void Insert()
        {
            using (AdventureWorksDbContext adventureWorks = new AdventureWorksDbContext())
            {
                ProductCategory category = new ProductCategory() { Name = "Category" };
                ProductSubcategory subcategory = new ProductSubcategory() { Name = "Subcategory" };
                category.ProductSubcategories.Add(subcategory);
                adventureWorks.ProductCategories.Add(category);

                Trace.WriteLine(category.ProductCategoryID); // 0.
                Trace.WriteLine(subcategory.ProductCategoryID); // 0.
                Trace.WriteLine(subcategory.ProductSubcategoryID); // 0.

                adventureWorks.SaveChanges();

                Trace.WriteLine(category.ProductCategoryID); // 0.
                Trace.WriteLine(subcategory.ProductCategoryID); // 0.
                Trace.WriteLine(subcategory.ProductSubcategoryID); // 0.
            }
        }
    }
}
