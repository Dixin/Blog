namespace Tutorial.LinqToEntities
{
#if EF
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using EntityEntry = System.Data.Entity.Infrastructure.DbEntityEntry;
#else
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
#endif

    internal static partial class Changes
    {
        internal static ProductCategory Create()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = new ProductCategory() { Name = "Create" };
                ProductSubcategory subcategory = new ProductSubcategory() { Name = "Create" };
                category.ProductSubcategories = new HashSet<ProductSubcategory>() { subcategory };
                // Equivalent to: subcategory.ProductCategory = category;
                category.ProductCategoryID.WriteLine(); // 0
                subcategory.ProductCategoryID.WriteLine(); // 0
                subcategory.ProductSubcategoryID.WriteLine(); // 0

                adventureWorks.ProductCategories.Add(category); // Track creation.
                // Equivalent to: adventureWorks.ProductSubcategories.Add(subcategory);
                adventureWorks.ChangeTracker.Entries()
                    .Count(tracking => tracking.State == EntityState.Added).WriteLine(); // 2
                object.ReferenceEquals(category.ProductSubcategories.Single(), subcategory).WriteLine(); // True

                adventureWorks.SaveChanges().WriteLine(); // 2
                // BEGIN TRANSACTION
                //    exec sp_executesql N'SET NOCOUNT ON;
                //    INSERT INTO [Production].[ProductCategory] ([Name])
                //    VALUES (@p0);
                //    SELECT [ProductCategoryID]
                //    FROM [Production].[ProductCategory]
                //    WHERE @@ROWCOUNT = 1 AND [ProductCategoryID] = scope_identity();
                //    ',N'@p0 nvarchar(50)',@p0=N'Create'
                //
                //    exec sp_executesql N'SET NOCOUNT ON;
                //    INSERT INTO [Production].[ProductCategory] ([Name])
                //    VALUES (@p0);
                //    SELECT [ProductCategoryID]
                //    FROM [Production].[ProductCategory]
                //    WHERE @@ROWCOUNT = 1 AND [ProductCategoryID] = scope_identity();
                //    ',N'@p0 nvarchar(50)',@p0=N'Create'
                // COMMIT TRANSACTION

                adventureWorks.ChangeTracker.Entries()
                    .Count(tracking => tracking.State != EntityState.Unchanged).WriteLine(); // 0
                category.ProductCategoryID.WriteLine(); // 5
                subcategory.ProductCategoryID.WriteLine(); // 5
                subcategory.ProductSubcategoryID.WriteLine(); // 38
                return category;
            } // Unit of work.
        }
    }

    internal static partial class Changes
    {
        internal static void Update(int categoryId, int subcategoryId)
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories.Find(categoryId);
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories.Find(subcategoryId);
                $"({subcategory.ProductSubcategoryID}, {subcategory.Name}, {subcategory.ProductCategoryID})"
                    .WriteLine(); // (48, Create, 25)
                subcategory.Name = "Update"; // Entity property update.
                subcategory.ProductCategory = category; // Relashionship (foreign key) update.
                adventureWorks.ChangeTracker.Entries().Count(tracking => tracking.State != EntityState.Unchanged)
                    .WriteLine(); // 1
                $"({subcategory.ProductSubcategoryID}, {subcategory.Name}, {subcategory.ProductCategoryID})"
                    .WriteLine(); // (48, Update, 1)
                adventureWorks.SaveChanges().WriteLine(); // 1
                // BEGIN TRANSACTION
                //    exec sp_executesql N'SET NOCOUNT ON;
                //    UPDATE [Production].[ProductSubcategory] SET [Name] = @p0, [ProductCategoryID] = @p1
                //    WHERE [ProductSubcategoryID] = @p2;
                //    SELECT @@ROWCOUNT;
                //    ',N'@p2 int,@p0 nvarchar(50),@p1 int',@p2=25,@p0=N'Update',@p1=25
                // COMMIT TRANSACTION
            } // Unit of work.
        }

        internal static void SaveNoChanges(int categoryId)
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories.Find(categoryId);
                string originalName = category.Name;
                category.Name = Guid.NewGuid().ToString(); // Entity property update.
                category.Name = originalName; // Entity property update.
                EntityEntry tracking = adventureWorks.ChangeTracker.Entries().Single();
                tracking.State.WriteLine(); // Unchanged
                adventureWorks.ChangeTracker.HasChanges().WriteLine(); // False
                adventureWorks.SaveChanges().WriteLine(); // 0
            } // Unit of work.
        }

        internal static void UpdateWithoutRead(int categoryId)
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = new ProductCategory()
                {
                    ProductCategoryID = categoryId,
                    Name = Guid.NewGuid().ToString() // Entity property update.
                };
                adventureWorks.ProductCategories.Attach(category); // Track entity.
                EntityEntry tracking = adventureWorks.ChangeTracker.Entries<ProductCategory>().Single();
                tracking.State.WriteLine(); // Unchanged
                tracking.State = EntityState.Modified;
                adventureWorks.SaveChanges().WriteLine(); // 1
                // BEGIN TRANSACTION
                //    exec sp_executesql N'SET NOCOUNT ON;
                //    UPDATE [Production].[ProductCategory] SET [Name] = @p0
                //    WHERE [ProductCategoryID] = @p1;
                //    SELECT @@ROWCOUNT;
                //    ',N'@p1 int,@p0 nvarchar(50)',@p1=25,@p0=N'513ce396-4a5e-4a86-9d82-46f284aa4f94'
                // COMMIT TRANSACTION
            } // Unit of work.
        }

        internal static void Delete(int subcategoryId)
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories.Find(subcategoryId);
                adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 1
                adventureWorks.ChangeTracker.Entries<ProductSubcategory>().Single().State.WriteLine(); // Unchanged
                adventureWorks.ProductSubcategories.Remove(subcategory); // Track deletion.
                adventureWorks.ChangeTracker.Entries<ProductSubcategory>().Single().State.WriteLine(); // Deleted
                adventureWorks.SaveChanges().WriteLine(); // 1
                // BEGIN TRANSACTION
                //    exec sp_executesql N'SET NOCOUNT ON;
                //    DELETE FROM [Production].[ProductSubcategory]
                //    WHERE [ProductSubcategoryID] = @p0;
                //    SELECT @@ROWCOUNT;
                //    ',N'@p0 int',@p0=48
                // COMMIT TRANSACTION
            } // Unit of work.
        }

        internal static void DeleteWithoutRead(int categoryId)
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = new ProductCategory() { ProductCategoryID = categoryId };
                adventureWorks.ProductCategories.Attach(category);
                adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 1
                adventureWorks.ChangeTracker.Entries<ProductCategory>().Single().State.WriteLine(); // Unchanged
                adventureWorks.ProductCategories.Remove(category); // Track deletion.
                adventureWorks.ChangeTracker.Entries<ProductCategory>().Single().State.WriteLine(); // Deleted
                adventureWorks.SaveChanges().WriteLine(); // 1
                //    BEGIN TRANSACTION
                //    exec sp_executesql N'SET NOCOUNT ON;
                //    DELETE FROM [Production].[ProductCategory]
                //    WHERE [ProductCategoryID] = @p0;
                //    SELECT @@ROWCOUNT;
                //    ',N'@p0 int',@p0=25
                // COMMIT TRANSACTION
            } // Unit of work.
        }

        internal static void DeleteWithRelationship(int categoryId)
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories.Find(categoryId);
                adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 1
                adventureWorks.ProductCategories.Remove(category);// Track deletion.
                adventureWorks.ChangeTracker.Entries().Count(tracking => tracking.State == EntityState.Deleted)
                    .WriteLine(); // 1
                adventureWorks.SaveChanges();
                // BEGIN TRANSACTION
                //    exec sp_executesql N'SET NOCOUNT ON;
                //    DELETE FROM [Production].[ProductCategory]
                //    WHERE [ProductCategoryID] = @p0;
                //    SELECT @@ROWCOUNT;
                // ',N'@p0 int',@p0=1
                // ROLLBACK TRANSACTION

                // DbUpdateException: An error occurred while updating the entries. See the inner exception for details.
                // ---> UpdateException: An error occurred while updating the entries. See the inner exception for details.
                // ---> SqlException: The DELETE statement conflicted with the REFERENCE constraint "FK_ProductSubcategory_ProductCategory_ProductCategoryID". The conflict occurred in database "D:\DIXIN\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF", table "Production.ProductSubcategory", column 'ProductCategoryID'.
            } // Unit of work.
        }

        internal static void DeleteCascade(int categoryId)
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories
#if !EF
                    .Include(entity => entity.ProductSubcategories)
#endif
                    .Single(entity => entity.ProductCategoryID == categoryId);
                ProductSubcategory subcategory = category.ProductSubcategories.Single();
                adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 2
                adventureWorks.ProductCategories.Remove(category); // Track deletion.
                // Optional: adventureWorks.ProductSubcategories.Remove(subcategory);
                adventureWorks.ChangeTracker.Entries().Count(tracking => tracking.State == EntityState.Deleted)
                    .WriteLine(); // 2
                adventureWorks.SaveChanges().WriteLine(); // 2
                // BEGIN TRANSACTION
                //    exec sp_executesql N'SET NOCOUNT ON;
                //    DELETE FROM [Production].[ProductSubcategory]
                //    WHERE [ProductSubcategoryID] = @p0;
                //    SELECT @@ROWCOUNT;
                //    ',N'@p0 int',@p0=49

                //    exec sp_executesql N'SET NOCOUNT ON;
                //    DELETE FROM [Production].[ProductCategory]
                //    WHERE [ProductCategoryID] = @p1;
                //    SELECT @@ROWCOUNT;
                //    ',N'@p1 int',@p1=26
                // COMMIT TRANSACTION
            } // Unit of work.
        }

        internal static void UntrackedChanges()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory untracked = adventureWorks.ProductCategories
                    .AsNoTracking()
                    .Single(category => category.Name == "Bikes");
                adventureWorks.ProductCategories.Remove(untracked); // Track no deletion.
                adventureWorks.SaveChanges().WriteLine();
#if EF
                // InvalidOperationException: The object cannot be deleted because it was not found in the ObjectStateManager.
#else
                // DbUpdateException: An error occurred while updating the entries. 
                // ---> SqlException: The DELETE statement conflicted with the REFERENCE constraint "FK_ProductSubcategory_ProductCategory_ProductCategoryID". The conflict occurred in database "C:\DATA\GITHUB\DATA\ADVENTUREWORKS_DATA.MDF", table "Production.ProductSubcategory", column 'ProductCategoryID'. The statement has been terminated.
#endif
            } // Unit of work.
        }
    }
}

#if DEMO
namespace Microsoft.EntityFrameworkCore
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public abstract class DbSet<TEntity> : IQueryable<TEntity> // Other interfaces.
        where TEntity : class
    {
        public virtual TEntity Find(params object[] keyValues);

        public virtual EntityEntry<TEntity> Add(TEntity entity);

        public virtual void AddRange(IEnumerable<TEntity> entities);

        public virtual EntityEntry<TEntity> Remove(TEntity entity);

        public virtual void RemoveRange(IEnumerable<TEntity> entities);

        // Other members.
    }
}

namespace Microsoft.EntityFrameworkCore
{
    using System;

    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public class DbContext : IDisposable, IInfrastructure<IServiceProvider>
    {
        public virtual DbSet<TEntity> Set<TEntity>() where TEntity : class;

        public virtual ChangeTracker ChangeTracker { get; }

        public virtual int SaveChanges();

        public virtual void Dispose();

        // Other members.
    }
}

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    using System.Collections.Generic;

    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

    public class ChangeTracker : IInfrastructure<IStateManager>
    {
        public virtual IEnumerable<EntityEntry> Entries();

        public virtual IEnumerable<EntityEntry<TEntity>> Entries<TEntity>() where TEntity : class;

        public virtual void DetectChanges();

        public virtual bool HasChanges();

        // Other members.
    }
}

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

    public class EntityEntry : IInfrastructure<InternalEntityEntry>
    {
        public virtual EntityState State { get; set; }

        public virtual object Entity { get; }

        public virtual PropertyEntry Property(string propertyName);

        public virtual PropertyValues CurrentValues { get; }

        public virtual PropertyValues OriginalValues { get; }

        public virtual PropertyValues GetDatabaseValues();

        public virtual void Reload();

        // Other members.
    }
}

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    public class EntityEntry<TEntity> : EntityEntry where TEntity : class
    {
        public virtual TEntity Entity { get; }

        // Other members.
    }
}

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Linq;

    public class DbSet<TEntity> : DbQuery<TEntity>, IQueryable<TEntity> // Other interfaces.
        where TEntity : class
    {
        public virtual TEntity Find(params object[] keyValues);

        public virtual TEntity Add(TEntity entity);

        public virtual IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities);

        public virtual TEntity Remove(TEntity entity);

        public virtual IEnumerable<TEntity> RemoveRange(IEnumerable<TEntity> entities);

        // Other members.
    }

    public class DbContext : IDisposable // Other interfaces.
    {
        public virtual DbSet<TEntity> Set<TEntity>() where TEntity : class;

        public DbChangeTracker ChangeTracker { get; }

        public virtual int SaveChanges();

        public void Dispose();

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
