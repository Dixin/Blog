namespace Dixin.Linq.EntityFramework
{
    using System;
#if NETFX
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
#endif
    using System.Diagnostics;
    using System.Linq;

#if !NETFX
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
#endif

    internal static partial class Changes
    {
        internal static ProductCategory Create()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                ProductSubcategory subcategory = new ProductSubcategory() { Name = nameof(ProductSubcategory) };
                adventureWorks.ProductSubcategories.Add(subcategory);
                subcategory.ProductCategory = category;
                // Equivalent to: category.ProductSubcategories.Add(subcategory);
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries()
                    .Count(tracking => tracking.State == EntityState.Added)); // 2
                Trace.WriteLine(category.ProductCategoryID); // 0
                Trace.WriteLine(subcategory.ProductCategoryID); // 0
                Trace.WriteLine(subcategory.ProductSubcategoryID); // 0

                Trace.WriteLine(adventureWorks.SaveChanges()); // 2
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries()
                    .Count(tracking => tracking.State != EntityState.Unchanged)); // 0
                Trace.WriteLine(category.ProductCategoryID); // 25
                Trace.WriteLine(subcategory.ProductCategoryID); // 25
                Trace.WriteLine(subcategory.ProductSubcategoryID); // 50
                return category;
            }
        }
    }

    internal static partial class Changes
    {
        internal static void Update()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories
                    .Single(entity => entity.Name == "Bikes");
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories
                    .Single(entity => entity.Name == nameof(ProductSubcategory));
                Trace.WriteLine(
                    $"({subcategory.ProductSubcategoryID}, {subcategory.Name}, {subcategory.ProductCategoryID})");
                // (48, ProductSubcategory, 25)

                subcategory.Name = "Update"; // Update property.
                subcategory.ProductCategory = category; // Update association (foreign key).
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries()
                    .Count(tracking => tracking.State != EntityState.Unchanged)); // 1
                Trace.WriteLine(
                    $"({subcategory.ProductSubcategoryID}, {subcategory.Name}, {subcategory.ProductCategoryID})");
                // (48, Update, 1)

                Trace.WriteLine(adventureWorks.SaveChanges()); // 1
            }
        }

        internal static void SaveNoChanges()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories.Find(1);
                string originalName = category.Name;
                category.Name = Guid.NewGuid().ToString(); // Update property value.
                category.Name = originalName; // Update property back to original value.
                Trace.WriteLine(adventureWorks.ChangeTracker.HasChanges()); // False
                Trace.WriteLine(adventureWorks.SaveChanges()); // 0
            }
        }

        internal static void UpdateWithoutRead(int categoryId)
        {
            ProductCategory category = new ProductCategory()
            {
                ProductCategoryID = categoryId,
                Name = Guid.NewGuid().ToString()
            };
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.ProductCategories.Attach(category);
#if NETFX
                DbEntityEntry<ProductCategory> tracking = adventureWorks.ChangeTracker.Entries<ProductCategory>()
                    .Single();
#else
                EntityEntry<ProductCategory> tracking = adventureWorks.ChangeTracker.Entries<ProductCategory>()
                    .Single();
#endif
                Trace.WriteLine(tracking.State); // Unchanged
                tracking.State = EntityState.Modified;
                Trace.WriteLine(adventureWorks.SaveChanges()); // 1
            }
        }

        internal static void Delete()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories
                    .OrderByDescending(entity => entity.ProductSubcategoryID).First();
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries().Count()); // 1
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries<ProductSubcategory>().Single().State); // Unchanged

                adventureWorks.ProductSubcategories.Remove(subcategory);
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries<ProductSubcategory>().Single().State); // Deleted
                Trace.WriteLine(adventureWorks.SaveChanges()); // 1
            }
        }

        internal static void DeleteWithoutRead(int categoryId)
        {
            ProductCategory category = new ProductCategory() { ProductCategoryID = categoryId };
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.ProductCategories.Attach(category);
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries().Count()); // 1
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries<ProductCategory>().Single().State); // Unchanged

                adventureWorks.ProductCategories.Remove(category);
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries<ProductCategory>().Single().State); // Deleted
                Trace.WriteLine(adventureWorks.SaveChanges()); // 1.
            }
        }

        internal static void DeleteWithAssociation()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories.Find(1);
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries().Count()); // 1

                adventureWorks.ProductCategories.Remove(category);
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries()
                    .Count(tracking => tracking.State == EntityState.Deleted)); // 1
                Trace.WriteLine(adventureWorks.SaveChanges());
                // System.Data.Entity.Infrastructure.DbUpdateException: An error occurred while updating the entries. See the inner exception for details.
                // ---> System.Data.Entity.Core.UpdateException: An error occurred while updating the entries. See the inner exception for details.
                // ---> System.Data.SqlClient.SqlException: The DELETE statement conflicted with the REFERENCE constraint "FK_ProductSubcategory_ProductCategory_ProductCategoryID". The conflict occurred in database "D:\DIXIN\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF", table "Production.ProductSubcategory", column 'ProductCategoryID'.
            }
        }

        internal static void DeleteAllAssociated()
        {
            Create(); // Create category "ProductCategory" and its subcategory "ProductSubcategory".
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories
                    .Single(entity => entity.Name == nameof(ProductCategory));
                ProductSubcategory subcategory = category.ProductSubcategories.Single();
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries().Count()); // 2

                adventureWorks.ProductCategories.Remove(category);
                // Optional: adventureWorks.ProductSubcategories.Remove(subcategory);
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries()
                    .Count(tracking => tracking.State == EntityState.Deleted)); // 2
                Trace.WriteLine(adventureWorks.SaveChanges()); // 2
            }
        }

        internal static void UntrackedChanges()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory untracked = adventureWorks.ProductCategories.AsNoTracking().First();
                adventureWorks.ProductCategories.Remove(untracked);
                Trace.WriteLine(adventureWorks.SaveChanges());
                // InvalidOperationException: The object cannot be deleted because it was not found in the ObjectStateManager.
            }
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
