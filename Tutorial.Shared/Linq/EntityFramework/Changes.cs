namespace Dixin.Linq.EntityFramework
{
    using System;
#if NETFX
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
#endif
    using System.Linq;

#if !NETFX
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
#endif

#if NETFX
    using EntityEntry = System.Data.Entity.Infrastructure.DbEntityEntry;
#endif

    internal static partial class Changes
    {
        internal static SupplierCategory Create(WideWorldImporters adventureWorks)
        {
            SupplierCategory category = new SupplierCategory() { SupplierCategoryName = nameof(SupplierCategory) };
            category.SupplierCategoryID.WriteLine(); // 0
            adventureWorks.SupplierCategories.Add(category);
            EntityEntry tracking = adventureWorks.ChangeTracker.Entries().Single();
            tracking.State.WriteLine(); // Added

            adventureWorks.SaveChanges().WriteLine(); // 1
            category.SupplierCategoryID.WriteLine(); // 25
            tracking.State.WriteLine(); // Unchanged
            return category;
        }

        internal static StockGroup CreateWithRelationship(WideWorldImporters adventureWorks)
        {
            const string groupName = "USB";
            StockGroup group = new StockGroup() { StockGroupName = groupName };
            adventureWorks.StockItems.Where(item => item.StockItemName.StartsWith(groupName))
                .Select(item => item.StockItemID)
                .ForEach(id => group.StockItemStockGroups.Add(new StockItemStockGroup() { StockItemID = id }));
            group.StockItemStockGroups.WriteLines(relationship => $"{relationship.StockGroupID}:{relationship.StockItemID}");
            // 0:9 0:10 0:11 0:15 0:8 0:12 0:14 0:5 0:6 0:7 0:13 0:4 0:1 0:2
            adventureWorks.StockGroups.Add(group);
            adventureWorks.ChangeTracker.Entries().Count(tracking => tracking.State != EntityState.Unchanged).WriteLine(); // 15

            adventureWorks.SaveChanges().WriteLine(); // 15

            adventureWorks.ChangeTracker.Entries().Count(tracking => tracking.State != EntityState.Unchanged).WriteLine(); // 0
            group.StockGroupID.WriteLine(); // 11
            group.StockItemStockGroups.WriteLines(relationship => $"{relationship.StockGroupID}:{relationship.StockItemID}");
            // 11:9 11:10 11:11 11:15 11:8 11:12 11:14 11:5 11:6 11:7 11:13 11:4 11:1 11:2
            return group;
        }
    }

    internal static partial class Changes
    {
        internal static void Update(WideWorldImporters adventureWorks)
        {
            SupplierCategory category = adventureWorks.SupplierCategories
                .Single(entity => entity.SupplierCategoryName == "Bikes");
            Supplier subcategory = adventureWorks.Suppliers
                .Single(entity => entity.SupplierName == nameof(Supplier));

            $"({subcategory.SupplierID}, {subcategory.SupplierName}, {subcategory.SupplierCategoryID})".WriteLine();
            // (48, ProductSubcategory, 25)

            subcategory.SupplierName = "Update"; // Update property.
            subcategory.SupplierCategory = category; // Update association (foreign key).
            adventureWorks.ChangeTracker.Entries().Count(tracking => tracking.State != EntityState.Unchanged)
                .WriteLine(); // 1

            $"({subcategory.SupplierID}, {subcategory.SupplierName}, {subcategory.SupplierCategoryID})"
                .WriteLine(); // (48, Update, 1)

            adventureWorks.SaveChanges().WriteLine(); // 1
        }

        internal static void SaveNoChanges(WideWorldImporters adventureWorks)
        {
            SupplierCategory category = adventureWorks.SupplierCategories.Find(1);
            string originalName = category.SupplierCategoryName;
            category.SupplierCategoryName = Guid.NewGuid().ToString(); // Update property value.
            category.SupplierCategoryName = originalName; // Update property back to original value.
            adventureWorks.ChangeTracker.HasChanges().WriteLine(); // False
            adventureWorks.SaveChanges().WriteLine(); // 0
        }

        internal static void UpdateWithoutRead(WideWorldImporters adventureWorks, int categoryId)
        {
            SupplierCategory category = new SupplierCategory()
            {
                SupplierCategoryID = categoryId,
                SupplierCategoryName = Guid.NewGuid().ToString()
            };
            adventureWorks.SupplierCategories.Attach(category);
            EntityEntry tracking = adventureWorks.ChangeTracker.Entries<SupplierCategory>()
                .Single();
            tracking.State.WriteLine(); // Unchanged
            tracking.State = EntityState.Modified;
            adventureWorks.SaveChanges().WriteLine(); // 1
        }

        internal static void Delete(WideWorldImporters adventureWorks)
        {
            Supplier subcategory = adventureWorks.Suppliers
                .OrderByDescending(entity => entity.SupplierID).First();
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 1
            adventureWorks.ChangeTracker.Entries<Supplier>().Single().State.WriteLine(); // Unchanged

            adventureWorks.Suppliers.Remove(subcategory);
            adventureWorks.ChangeTracker.Entries<Supplier>().Single().State.WriteLine(); // Deleted
            adventureWorks.SaveChanges().WriteLine(); // 1
        }

        internal static void DeleteWithoutRead(WideWorldImporters adventureWorks, int categoryId)
        {
            SupplierCategory category = new SupplierCategory() { SupplierCategoryID = categoryId };
            adventureWorks.SupplierCategories.Attach(category);
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 1
            adventureWorks.ChangeTracker.Entries<SupplierCategory>().Single().State.WriteLine(); // Unchanged

            adventureWorks.SupplierCategories.Remove(category);
            adventureWorks.ChangeTracker.Entries<SupplierCategory>().Single().State.WriteLine(); // Deleted
            adventureWorks.SaveChanges().WriteLine(); // 1.
        }

        internal static void DeleteWithAssociation(WideWorldImporters adventureWorks)
        {
            SupplierCategory category = adventureWorks.SupplierCategories.Find(1);
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 1

            adventureWorks.SupplierCategories.Remove(category);
            adventureWorks.ChangeTracker.Entries().Count(tracking => tracking.State == EntityState.Deleted)
                .WriteLine(); // 1
            adventureWorks.SaveChanges().WriteLine();
            // System.Data.Entity.Infrastructure.DbUpdateException: An error occurred while updating the entries. See the inner exception for details.
            // ---> System.Data.Entity.Core.UpdateException: An error occurred while updating the entries. See the inner exception for details.
            // ---> System.Data.SqlClient.SqlException: The DELETE statement conflicted with the REFERENCE constraint "FK_ProductSubcategory_ProductCategory_ProductCategoryID". The conflict occurred in database "D:\DIXIN\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF", table "Production.ProductSubcategory", column 'ProductCategoryID'.
        }

        internal static void DeleteAllAssociated(WideWorldImporters adventureWorks)
        {
            Create(adventureWorks); // Create category "ProductCategory" and its subcategory "ProductSubcategory".
            SupplierCategory category = adventureWorks.SupplierCategories
#if !NETFX
                .Include(entity => entity.Suppliers)
#endif
                .Single(entity => entity.SupplierCategoryName == nameof(SupplierCategory));
            Supplier subcategory = category.Suppliers.Single();
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 2

            adventureWorks.SupplierCategories.Remove(category);
            // Optional: adventureWorks.ProductSubcategories.Remove(subcategory);
            adventureWorks.ChangeTracker.Entries().Count(tracking => tracking.State == EntityState.Deleted)
                .WriteLine(); // 2
            adventureWorks.SaveChanges().WriteLine(); // 2
        }

        internal static void UntrackedChanges(WideWorldImporters adventureWorks)
        {
            SupplierCategory untracked = adventureWorks.SupplierCategories.AsNoTracking().First();
            adventureWorks.SupplierCategories.Remove(untracked);
            adventureWorks.SaveChanges().WriteLine();
#if NETFX
            // InvalidOperationException: The object cannot be deleted because it was not found in the ObjectStateManager.
#else
            // Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while updating the entries. 
            // ---> System.Data.SqlClient.SqlException: The DELETE statement conflicted with the REFERENCE constraint "FK_ProductSubcategory_ProductCategory_ProductCategoryID". The conflict occurred in database "C:\DATA\GITHUB\DATA\ADVENTUREWORKS_DATA.MDF", table "Production.ProductSubcategory", column 'ProductCategoryID'. The statement has been terminated.
#endif
        }
    }
}

#if DEMO
namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Linq;

    public class DbSet<TEntity> : DbQuery<TEntity>, IQueryable<TEntity> // Other interfaces.
        where TEntity : class
    {
        public virtual TEntity Add(TEntity entity);

        public virtual IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities);

        public virtual TEntity Find(params object[] keyValues);

        public virtual TEntity Remove(TEntity entity);

        public virtual IEnumerable<TEntity> RemoveRange(IEnumerable<TEntity> entities);

        // Other members.
    }
}

namespace System.Data.Entity
{
    using System.Data.Entity.Infrastructure;

    public class DbContext : IDisposable // Other interfaces.
    {
        public DbChangeTracker ChangeTracker { get; }

        public void Dispose();

        public virtual int SaveChanges();

        public virtual DbSet Set(Type entityType);

        // Other members.
    }
}

namespace System.Data.SqlClient
{
    internal static class TdsEnums
    {
        internal enum TransactionManagerIsolationLevel
        {
            Unspecified, // 0
            ReadUncommitted, // 1
            ReadCommitted, // 2
            RepeatableRead, // 3
            Serializable, // 4
            Snapshot // 5
        }
    }
}
#endif
