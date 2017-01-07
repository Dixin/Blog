namespace Dixin.Linq.EntityFramework
{
#if NETFX
    using System.Data.Entity;
#endif
    using System.Linq;

#if !NETFX
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
#endif

    internal static partial class Tracking
    {
        internal static void EntitiesFromSameDbContext(WideWorldImporters adventureWorks)
        {
            StockItem productById = adventureWorks.StockItems
                .Single(product => product.StockItemID == 1);
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 1

            StockItem productByName = adventureWorks.StockItems
                .Single(product => product.StockItemName == "USB missile launcher (Green)");
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 1
            object.ReferenceEquals(productById, productByName).WriteLine(); // True
        }
    }

    internal static partial class Tracking
    {
        internal static void ObjectsFromSameDbContext(WideWorldImporters adventureWorks)
        {
            var productById = adventureWorks.StockItems
                .Select(product => new { ProductID = product.StockItemID, Name = product.StockItemName })
                .Single(product => product.ProductID == 1);
            var productByName = adventureWorks.StockItems
                .Select(product => new { ProductID = product.StockItemID, Name = product.StockItemName })
                .Single(product => product.Name == "USB missile launcher (Green)");
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 0
            object.ReferenceEquals(productById, productByName).WriteLine(); // False
        }

        internal static void EntitiesFromDbContexts()
        {
            StockItem productById;
            StockItem productByName;
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                productById = adventureWorks.StockItems.Single(product => product.StockItemID == 1);
            }
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                productByName = adventureWorks.StockItems.Single(product => product.StockItemName == "USB missile launcher (Green)");
            }
            object.ReferenceEquals(productById, productByName).WriteLine(); // False.
        }

        internal static void EntityChanges(WideWorldImporters adventureWorks)
        {
            StockItem toCreate = new StockItem() { StockItemName = nameof(toCreate), UnitPrice = 1 };
            adventureWorks.StockItems.Add(toCreate); // Create entity.
            StockItem read = adventureWorks.StockItems.Single(product => product.StockItemID == 1); // Read entity.
            IQueryable<StockItem> toUpdate = adventureWorks.StockItems
                .Where(product => product.StockItemName.Contains("HL"));
            toUpdate.ForEach(product => product.UnitPrice += 100); // Update entities.
            IQueryable<StockItem> toDelete = adventureWorks.StockItems
                .Where(product => product.StockItemName.Contains("ML"));
            adventureWorks.StockItems.RemoveRange(toDelete); // Delete entities.

            adventureWorks.ChangeTracker.HasChanges().WriteLine(); // True
            adventureWorks.ChangeTracker.Entries<StockItem>().ForEach(tracking =>
            {
                StockItem changed = tracking.Entity;
                switch (tracking.State)
                {
                    case EntityState.Added:
                    case EntityState.Deleted:
                    case EntityState.Unchanged:
                        $"{tracking.State}: ({changed.StockItemID}, {changed.StockItemName}, {changed.UnitPrice})".WriteLine();
                        break;
                    case EntityState.Modified:
#if NETFX
                        StockItem original = (StockItem)tracking.OriginalValues.ToObject();
#else
                        PropertyValues originalValues = tracking.OriginalValues.Clone();
                        originalValues.SetValues(tracking.OriginalValues);
                        StockItem original = (StockItem)originalValues.ToObject();
#endif
                        $"{tracking.State}: ({original.StockItemID}, {original.StockItemName}, {original.UnitPrice}) => ({changed.StockItemID}, {changed.StockItemName}, {changed.UnitPrice})"
                            .WriteLine();
                        break;
                }
            });
            // Added: (0, toCreate, 1)
            // Modified: (951, HL Crankset, 404.9900) => (951, HL Crankset, 504.9900)
            // Modified: (996, HL Bottom Bracket, 121.4900) => (996, HL Bottom Bracket, 221.4900)
            // Deleted: (950, ML Crankset, 256.4900)
            // Deleted: (995, ML Bottom Bracket, 101.2400)
            // Unchanged: (1, Road-750 Black, 52, 539.9900)
        }

        internal static void Attach(WideWorldImporters adventureWorks)
        {
            StockItem onTheFly = new StockItem() { StockItemID = 950, StockItemName = "ML Crankset", UnitPrice = 539.99M };
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 0

            adventureWorks.StockItems.Attach(onTheFly);
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 1
            adventureWorks.ChangeTracker.Entries<StockItem>().Single().State.WriteLine(); // Unchanged
            onTheFly.StockItemName = "After attaching";
            adventureWorks.ChangeTracker.Entries<StockItem>().Single().State.WriteLine(); // Modified
            adventureWorks.ChangeTracker.Entries<StockItem>().WriteLines(tracking =>
                $"{tracking.State}: {tracking.OriginalValues[nameof(StockItem.StockItemName)]} => {tracking.CurrentValues[nameof(StockItem.StockItemName)]}");
            // Modified: ML Crankset => After attaching
        }

        internal static void AssociationChanges(WideWorldImporters adventureWorks)
        {
            Supplier subcategory = adventureWorks.Suppliers
                .Include(entity => entity.StockItems).Single(entity => entity.SupplierID == 8);
            subcategory.StockItems.Count.WriteLine(); // 2
            subcategory.StockItems
                .All(product => product.Supplier == subcategory).WriteLine(); // True

            subcategory.StockItems.Clear();
            // Equivalent to: subcategory.Products.ForEach(product => product.ProductSubcategory = null);
            subcategory.StockItems.Count.WriteLine(); // 0
            subcategory.StockItems
                .All(product => product.Supplier == null).WriteLine(); // True
            adventureWorks.ChangeTracker.Entries<StockItem>().WriteLines(tracking =>
            {
                StockItem original = (StockItem)tracking.OriginalValues.ToObject();
                StockItem changed = tracking.Entity;
                return $"{tracking.State}: ({original.StockItemID}, {original.StockItemName}, {original.SupplierID}) => ({changed.StockItemID}, {changed.StockItemName}, {changed.SupplierID})";
            });
            // Modified: (950, ML Crankset, 8) => (950, ML Crankset, )
            // Modified: (951, HL Crankset, 8) => (951, HL Crankset, )
        }

        internal static void AsNoTracking(WideWorldImporters adventureWorks)
        {
            StockItem untracked = adventureWorks.StockItems.AsNoTracking().First();
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 0
        }

        internal static void DetectChanges(WideWorldImporters adventureWorks)
        {
#if NETFX
            adventureWorks.Configuration.AutoDetectChangesEnabled = false;
#else
            adventureWorks.ChangeTracker.AutoDetectChangesEnabled = false;
#endif
            StockItem product = adventureWorks.StockItems.First();
            product.UnitPrice += 100;
            adventureWorks.ChangeTracker.HasChanges().WriteLine(); // False
            adventureWorks.ChangeTracker.DetectChanges();
            adventureWorks.ChangeTracker.HasChanges().WriteLine(); // True
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
