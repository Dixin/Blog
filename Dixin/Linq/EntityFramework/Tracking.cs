namespace Dixin.Linq.EntityFramework
{
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Linq;

    internal static partial class Tracking
    {
        internal static void EntitiesFromSameDbContext()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Product productById = adventureWorks.Products
                    .Single(product => product.ProductID == 999);
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries().Count()); // 1

                Product productByName = adventureWorks.Products
                    .Single(product => product.Name == "Road-750 Black, 52");
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries().Count()); // 1
                Trace.WriteLine(object.ReferenceEquals(productById, productByName)); // True
            }
        }
    }

    internal static partial class Tracking
    {
        internal static void ObjectsFromSameDbContext()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                var productById = adventureWorks.Products
                    .Select(product => new { ProductID = product.ProductID, Name = product.Name })
                    .Single(product => product.ProductID == 999);
                var productByName = adventureWorks.Products
                    .Select(product => new { ProductID = product.ProductID, Name = product.Name })
                    .Single(product => product.Name == "Road-750 Black, 52");
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries().Count()); // 0
                Trace.WriteLine(object.ReferenceEquals(productById, productByName)); // False
            }
        }

        internal static void EntitiesFromDbContexts()
        {
            Product productById;
            Product productByName;
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                productById = adventureWorks.Products.Single(product => product.ProductID == 999);
            }
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                productByName = adventureWorks.Products.Single(product => product.Name == "Road-750 Black, 52");
            }
            Trace.WriteLine(object.ReferenceEquals(productById, productByName)); // False.
        }

        internal static void EntityChanges()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Product toCreate = new Product() { Name = nameof(toCreate), ListPrice = 1 };
                adventureWorks.Products.Add(toCreate); // Create entity.
                Product read = adventureWorks.Products.Single(product => product.ProductID == 999); // Read entity.
                IQueryable<Product> toUpdate = adventureWorks.Products
                    .Where(product => product.Name.Contains("HL"));
                toUpdate.ForEach(product => product.ListPrice += 100); // Update entities.
                IQueryable<Product> toDelete = adventureWorks.Products
                    .Where(product => product.Name.Contains("ML"));
                adventureWorks.Products.RemoveRange(toDelete); // Delete entities.

                Trace.WriteLine(adventureWorks.ChangeTracker.HasChanges()); // True
                adventureWorks.ChangeTracker.Entries<Product>().ForEach(tracking =>
                    {
                        Product changed = tracking.Entity;
                        switch (tracking.State)
                        {
                            case EntityState.Added:
                            case EntityState.Deleted:
                            case EntityState.Unchanged:
                                Trace.WriteLine($"{tracking.State}: ({changed.ProductID}, {changed.Name}, {changed.ListPrice})");
                                break;
                            case EntityState.Modified:
                                Product original = tracking.OriginalValues.ToObject() as Product;
                                Trace.WriteLine(
                                    $"{tracking.State}: ({original.ProductID}, {original.Name}, {original.ListPrice}) => ({changed.ProductID}, {changed.Name}, {changed.ListPrice})");
                                break;
                        }
                    });
                // Added: (0, toCreate, 1)
                // Modified: (951, HL Crankset, 404.9900) => (951, HL Crankset, 504.9900)
                // Modified: (996, HL Bottom Bracket, 121.4900) => (996, HL Bottom Bracket, 221.4900)
                // Deleted: (950, ML Crankset, 256.4900)
                // Deleted: (995, ML Bottom Bracket, 101.2400)
                // Unchanged: (999, Road-750 Black, 52, 539.9900)
            }
        }

        internal static void Attach()
        {
            Product onTheFly = new Product() { ProductID = 950, Name = "ML Crankset", ListPrice = 539.99M };
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries().Count()); // 0

                adventureWorks.Products.Attach(onTheFly);
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries().Count()); // 1
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries<Product>().Single().State); // Unchanged
                onTheFly.Name = "After attaching";
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries<Product>().Single().State); // Modified
                adventureWorks.ChangeTracker.Entries<Product>().ForEach(tracking => Trace.WriteLine(
                    $"{tracking.State}: {tracking.OriginalValues[nameof(Product.Name)]} => {tracking.CurrentValues[nameof(Product.Name)]}"));
                // Modified: ML Crankset => After attaching
            }
        }

        internal static void AssociationChanges()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories
                    .Include(entity => entity.Products).Single(entity => entity.ProductSubcategoryID == 8);
                Trace.WriteLine(subcategory.Products.Count); // 2
                Trace.WriteLine(subcategory.Products
                    .All(product => product.ProductSubcategory == subcategory)); // True

                subcategory.Products.Clear();
                // Equivalent to: subcategory.Products.ForEach(product => product.ProductSubcategory = null);
                Trace.WriteLine(subcategory.Products.Count); // 0
                Trace.WriteLine(subcategory.Products
                    .All(product => product.ProductSubcategory == null)); // True
                adventureWorks.ChangeTracker.Entries<Product>().ForEach(tracking =>
                    {
                        Product original = tracking.OriginalValues.ToObject() as Product;
                        Product changed = tracking.Entity;
                        Trace.WriteLine(
                            $"{tracking.State}: ({original.ProductID}, {original.Name}, {original.ProductSubcategoryID}) => ({changed.ProductID}, {changed.Name}, {changed.ProductSubcategoryID})");
                    });
                // Modified: (950, ML Crankset, 8) => (950, ML Crankset, )
                // Modified: (951, HL Crankset, 8) => (951, HL Crankset, )
            }
        }

        internal static void AsNoTracking()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Product untracked = adventureWorks.Products.AsNoTracking().First();
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries().Count()); // 0
            }
        }

        internal static void DetectChanges()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.Configuration.AutoDetectChangesEnabled = false;
                Product product = adventureWorks.Products.First();
                product.ListPrice += 100;
                Trace.WriteLine(adventureWorks.ChangeTracker.HasChanges()); // False
                adventureWorks.ChangeTracker.DetectChanges();
                Trace.WriteLine(adventureWorks.ChangeTracker.HasChanges()); // True
            }
        }
    }
}

#if DEMO
namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;

    public class DbChangeTracker
    {
        public void DetectChanges();

        public IEnumerable<DbEntityEntry> Entries();

        public IEnumerable<DbEntityEntry<TEntity>> Entries<TEntity>() where TEntity : class;

        public bool HasChanges();

        // Other members.
    }
}

namespace System.Data.Entity.Infrastructure
{
    public class DbEntityEntry
    {
        public DbPropertyValues CurrentValues { get; }

        public object Entity { get; }

        public DbPropertyValues OriginalValues { get; }

        public EntityState State { get; set; }

        public DbPropertyValues GetDatabaseValues();

        public DbPropertyEntry Property(string propertyName);

        public void Reload();

        public DbEntityEntry<TEntity> Cast<TEntity>() where TEntity : class;

        // Other members.
    }
}

namespace System.Data.Entity.Infrastructure
{
    public class DbEntityEntry<TEntity> where TEntity : class
    {
        public DbPropertyValues CurrentValues { get; }

        public TEntity Entity { get; }

        public DbPropertyValues OriginalValues { get; }

        public EntityState State { get; set; }

        public DbPropertyValues GetDatabaseValues();

        public DbPropertyEntry Property(string propertyName);

        public void Reload();

        public static implicit operator DbEntityEntry(DbEntityEntry<TEntity> entry);

        // Other members.
    }
}
#endif
