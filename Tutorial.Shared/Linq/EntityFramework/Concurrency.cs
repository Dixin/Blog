namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
#if NETFX
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
#endif
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

#if !NETFX
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
#endif
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

#if NETFX
    using EntityEntry = System.Data.Entity.Infrastructure.DbEntityEntry;
    using PropertyValues = System.Data.Entity.Infrastructure.DbPropertyValues;
#endif

    public static partial class StringExtensions
    {
        public static string ToRowVersionString(this byte[] rowVersion) =>
            $"0x{BitConverter.ToString(rowVersion).Replace("-", string.Empty)}";
            // $"0x{BitConverter.ToUInt64(rowVersion.Reverse().ToArray(), 0).ToString("X16")}";
    }

    internal partial class DbReaderWriter : IDisposable
    {
        private readonly DbContext context;

        internal DbReaderWriter(DbContext context)
        {
            this.context = context;
        }

        internal TEntity Read<TEntity>
            (params object[] keys) where TEntity : class => this.context.Set<TEntity>().Find(keys);

        internal int Write(Action change)
        {
            change();
            return this.context.SaveChanges();
        }

        internal DbSet<TEntity> Set<TEntity>() where TEntity : class => this.context.Set<TEntity>();

        public void Dispose() => this.context.Dispose();
    }

    internal static partial class Concurrency
    {
        internal static void NoCheck(AdventureWorks adventureWorks1, AdventureWorks adventureWorks2) // Check no column, last client wins.
        {
            const int id = 1;
            using (DbReaderWriter readerWriter1 = new DbReaderWriter(adventureWorks1))
            using (DbReaderWriter readerWriter2 = new DbReaderWriter(adventureWorks2))
            {
                ProductCategory categoryCopy1 = readerWriter1.Read<ProductCategory>(id);
                ProductCategory categoryCopy2 = readerWriter2.Read<ProductCategory>(id);

                readerWriter1.Write(() => categoryCopy1.Name = nameof(readerWriter1));
                readerWriter2.Write(() => categoryCopy2.Name = nameof(readerWriter2)); // Win.
            }
            using (DbReaderWriter readerWriter3 = new DbReaderWriter(new AdventureWorks()))
            {
                ProductCategory category3 = readerWriter3.Read<ProductCategory>(id);
                Trace.WriteLine(category3.Name); // readerWriter2
            }
        }
    }

    internal static partial class Concurrency
    {
        internal static void ConcurrencyCheck(AdventureWorks adventureWorks1, AdventureWorks adventureWorks2)
        {
            using (DbReaderWriter readerWriter1 = new DbReaderWriter(adventureWorks1))
            using (DbReaderWriter readerWriter2 = new DbReaderWriter(adventureWorks2))
            {
                const int id = 1;
                ProductPhoto photoCopy1 = readerWriter1.Read<ProductPhoto>(id);
                ProductPhoto photoCopy2 = readerWriter2.Read<ProductPhoto>(id);

                readerWriter1.Write(() =>
                {
                    photoCopy1.LargePhotoFileName = nameof(readerWriter1);
                    photoCopy1.ModifiedDate = DateTime.Now;
                });
                readerWriter2.Write(() =>
                {
                    photoCopy2.LargePhotoFileName = nameof(readerWriter2);
                    photoCopy2.ModifiedDate = DateTime.Now;
                });
                // System.Data.Entity.Infrastructure.DbUpdateConcurrencyException: Store update, insert, or delete statement affected an unexpected number of rows (0).Entities may have been modified or deleted since entities were loaded.See http://go.microsoft.com/fwlink/?LinkId=472540 for information on understanding and handling optimistic concurrency exceptions. 
                // ---> System.Data.Entity.Core.OptimisticConcurrencyException: Store update, insert, or delete statement affected an unexpected number of rows (0).Entities may have been modified or deleted since entities were loaded.See http://go.microsoft.com/fwlink/?LinkId=472540 for information on understanding and handling optimistic concurrency exceptions.
            }
        }
    }

    public partial class Product
    {
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    internal static partial class Concurrency
    {
        internal static void RowVersion()
        {
            using (DbReaderWriter readerWriter1 = new DbReaderWriter(new AdventureWorks()))
            using (DbReaderWriter readerWriter2 = new DbReaderWriter(new AdventureWorks()))
            {
                const int id = 999;
                Product productCopy1 = readerWriter1.Read<Product>(id);
                Trace.WriteLine(productCopy1.RowVersion.ToRowVersionString()); // 0x0000000000000803
                Product productCopy2 = readerWriter2.Read<Product>(id);
                Trace.WriteLine(productCopy2.RowVersion.ToRowVersionString()); // 0x0000000000000803

                readerWriter1.Write(() => productCopy1.Name = nameof(readerWriter1));
                Trace.WriteLine(productCopy1.RowVersion.ToRowVersionString()); // 0x00000000000324B1
                readerWriter2.Write(() => readerWriter2.Set<Product>().Remove(productCopy2));
                // System.Data.Entity.Infrastructure.DbUpdateConcurrencyException: Store update, insert, or delete statement affected an unexpected number of rows (0). Entities may have been modified or deleted since entities were loaded. See http://go.microsoft.com/fwlink/?LinkId=472540 for information on understanding and handling optimistic concurrency exceptions.
                // ---> System.Data.Entity.Core.OptimisticConcurrencyException: Store update, insert, or delete statement affected an unexpected number of rows (0). Entities may have been modified or deleted since entities were loaded. See http://go.microsoft.com/fwlink/?LinkId=472540 for information on understanding and handling optimistic concurrency exceptions.
            }
        }
    }

    internal partial class DbReaderWriter
    {
        internal int Write
            (Action change, Action<DbUpdateConcurrencyException> handleDbUpdateConcurrencyException, int retryCount = 3)
        {
            change();
            for (int retry = 1; retry < retryCount; retry++)
            {
                try
                {
                    return this.context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    handleDbUpdateConcurrencyException(exception);
                }
            }
            return this.context.SaveChanges();
        }
    }

    internal static partial class Concurrency
    {
        internal static void UpdateProduct(Action<EntityEntry> resolveProductConflict)
        {
            const int id = 950;
            using (DbReaderWriter readerWriter1 = new DbReaderWriter(new AdventureWorks()))
            using (DbReaderWriter readerWriter2 = new DbReaderWriter(new AdventureWorks()))
            {
                Product productCopy1 = readerWriter1.Read<Product>(id);
                Product productCopy2 = readerWriter2.Read<Product>(id);
                readerWriter1.Write(() =>
                    {
                        productCopy1.Name = nameof(readerWriter1);
                        productCopy1.ListPrice = 100;
                    });
                readerWriter2.Write(
                    change: () =>
                        {
                            productCopy2.Name = nameof(readerWriter2);
                            productCopy2.ProductSubcategoryID = 1;
                        },
                    handleDbUpdateConcurrencyException: exception =>
                        {
                            // Logging.
                            EntityEntry tracking = exception.Entries.Single();
                            Product original = (Product)tracking.OriginalValues.ToObject();
                            Product updateTo = (Product)tracking.CurrentValues.ToObject();
                            Product database = productCopy1; // Values saved in database.

                            Trace.WriteLine(
                                $"Original:  ({original.Name},   {original.ListPrice}, {original.ProductSubcategoryID}, {original.RowVersion.ToRowVersionString()})");
                            Trace.WriteLine(
                                $"Database:  ({database.Name}, {database.ListPrice}, {database.ProductSubcategoryID}, {database.RowVersion.ToRowVersionString()})");
                            Trace.WriteLine(
                                $"Update to: ({updateTo.Name}, {updateTo.ListPrice}, {updateTo.ProductSubcategoryID})");

                            // Resolve product conflict.
                            resolveProductConflict(tracking);
                        });
            }

            using (DbReaderWriter readerWriter3 = new DbReaderWriter(new AdventureWorks()))
            {
                Product resolved = readerWriter3.Read<Product>(id);
                Trace.WriteLine(
                    $"Resolved:  ({resolved.Name}, {resolved.ListPrice}, {resolved.ProductSubcategoryID}, {resolved.RowVersion.ToRowVersionString()})");
            }
        }
    }

    internal partial class DbReaderWriter
    {
        internal int WriteDatabaseWins(Action change)
        {
            change();
            try
            {
                return this.context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return 0;
                // this.context is in a corrupted state.
            }
        }
    }

    internal static partial class Concurrency
    {
        internal static void UpdateProductDatabaseWins() =>
            UpdateProduct(resolveProductConflict: tracking =>
                {
                    Trace.WriteLine(tracking.State); // Modified
                    Trace.WriteLine(tracking.Property(nameof(Product.Name)).IsModified); // True
                    Trace.WriteLine(tracking.Property(nameof(Product.ListPrice)).IsModified); // False
                    Trace.WriteLine(tracking.Property(nameof(Product.ProductSubcategoryID)).IsModified); // True

                    tracking.Reload();

                    Trace.WriteLine(tracking.State); // Unchanged
                    Trace.WriteLine(tracking.Property(nameof(Product.Name)).IsModified); // False
                    Trace.WriteLine(tracking.Property(nameof(Product.ListPrice)).IsModified); // False
                    Trace.WriteLine(tracking.Property(nameof(Product.ProductSubcategoryID)).IsModified); // False
                });
        // Original:  (ML Crankset,   256.4900, 8, 0x00000000000007D1)
        // Database:  (readerWriter1, 100.0000, 8, 0x0000000000036335)
        // Update to: (readerWriter2, 256.4900, 1)
        // Resolved:  (readerWriter1, 100.0000, 8, 0x0000000000036335)

        internal static void UpdateProductClientWins() =>
            UpdateProduct(resolveProductConflict: tracking =>
                {
                    PropertyValues databaseValues = tracking.GetDatabaseValues();
                    // Refresh original values, which go to WHERE clause.
                    tracking.OriginalValues.SetValues(databaseValues);

                    Trace.WriteLine(tracking.State); // Modified
                    Trace.WriteLine(tracking.Property(nameof(Product.Name)).IsModified); // True
                    Trace.WriteLine(tracking.Property(nameof(Product.ListPrice)).IsModified); // True
                    Trace.WriteLine(tracking.Property(nameof(Product.ProductSubcategoryID)).IsModified); // True
                });
        // Original:  (ML Crankset,   256.4900, 8, 0x00000000000007D1)
        // Database:  (readerWriter1, 100.0000, 8, 0x0000000000036336)
        // Update to: (readerWriter2, 256.4900, 1)
        // Resolved:  (readerWriter2, 256.4900, 1, 0x0000000000036337)

        internal static void UpdateProductMergeClientAndDatabase() =>
            UpdateProduct(resolveProductConflict: tracking =>
                {
                    PropertyValues databaseValues = tracking.GetDatabaseValues();
                    PropertyValues originalValues = tracking.OriginalValues.Clone();
                    // Refresh original values, which go to WHERE clause.
                    tracking.OriginalValues.SetValues(databaseValues);
                    // If database has an different value for a property, then retain the database value.
#if NETFX
                    databaseValues.PropertyNames // Navigation properties are not included.
                        .Where(property => !object.Equals(originalValues[property], databaseValues[property]))
                        .ForEach(property => tracking.Property(property).IsModified = false);
#else
                    databaseValues.Properties // Navigation properties are not included.
                        .Where(property => !object.Equals(originalValues[property.Name], databaseValues[property.Name]))
                        .ForEach(property => tracking.Property(property.Name).IsModified = false);
#endif
                    Trace.WriteLine(tracking.State); // Modified
                    Trace.WriteLine(tracking.Property(nameof(Product.Name)).IsModified); // False
                    Trace.WriteLine(tracking.Property(nameof(Product.ListPrice)).IsModified); // False
                    Trace.WriteLine(tracking.Property(nameof(Product.ProductSubcategoryID)).IsModified); // True
                });
        // Original:  (ML Crankset,   256.4900, 8, 0x00000000000007D1)
        // Database:  (readerWriter1, 100.0000, 8, 0x0000000000036338)
        // Update to: (readerWriter2, 256.4900, 1)
        // Resolved:  (readerWriter1, 100.0000, 1, 0x0000000000036339)

        internal static void DeleteProductDatabaseWins(Action<EntityEntry> resolveProductConflict)
        {
            const int id = 950;
            using (DbReaderWriter readerWriter1 = new DbReaderWriter(new AdventureWorks()))
            using (DbReaderWriter readerWriter2 = new DbReaderWriter(new AdventureWorks()))
            {
                Product productCopy1 = readerWriter1.Read<Product>(id);
                Product productCopy2 = readerWriter2.Read<Product>(id);
                readerWriter1.Write(() => readerWriter1.Set<Product>().Remove(productCopy1));
                readerWriter2.Write(
                    change: () => readerWriter2.Set<Product>().Remove(productCopy2),
                    handleDbUpdateConcurrencyException: exception =>
                        {
                            EntityEntry tracking = exception.Entries.Single();
                            tracking.Reload();
                        });
            }

            using (DbReaderWriter readerWriter3 = new DbReaderWriter(new AdventureWorks()))
            {
                Trace.WriteLine(readerWriter3.Read<Product>(id) == null);
            }
        }

        internal static void DeleteProductClientWins(Action<EntityEntry> resolveProductConflict)
        {
            const int id = 950;
            using (DbReaderWriter readerWriter1 = new DbReaderWriter(new AdventureWorks()))
            using (DbReaderWriter readerWriter2 = new DbReaderWriter(new AdventureWorks()))
            {
                Product productCopy1 = readerWriter1.Read<Product>(id);
                Product productCopy2 = readerWriter2.Read<Product>(id);
                readerWriter1.Write(() => readerWriter1.Set<Product>().Remove(productCopy1));
                readerWriter2.Write(
                    change: () => readerWriter2.Set<Product>().Remove(productCopy2),
                    handleDbUpdateConcurrencyException: exception =>
                    {
                        EntityEntry tracking = exception.Entries.Single();
                        tracking.Reload();
                    });
            }

            using (DbReaderWriter readerWriter3 = new DbReaderWriter(new AdventureWorks()))
            {
                Trace.WriteLine(readerWriter3.Read<Product>(id) == null);
            }
        }
    }

    public static partial class DbContextExtensions
    {
        public static int SaveChanges(
            this DbContext context, Action<IEnumerable<EntityEntry>> resolveConflicts, int retryCount = 3)
        {
            if (retryCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount), $"{retryCount} must be greater than 0.");
            }

            for (int retry = 1; retry < retryCount; retry++)
            {
                try
                {
                    return context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException exception) when (retry < retryCount)
                {
                    resolveConflicts(exception.Entries);
                }
            }
            return context.SaveChanges();
        }
    }

    public class TransientDetection<TException> : ITransientErrorDetectionStrategy
        where TException : Exception
    {
        public bool IsTransient(Exception ex) => ex is TException;
    }

    public static partial class DbContextExtensions
    {
        public static int SaveChanges(
            this DbContext context, Action<IEnumerable<EntityEntry>> resolveConflicts, RetryStrategy retryStrategy)
        {
            RetryPolicy retryPolicy = new RetryPolicy(
                new TransientDetection<DbUpdateConcurrencyException>(), retryStrategy);
            retryPolicy.Retrying += (sender, e) =>
                resolveConflicts(((DbUpdateConcurrencyException)e.LastException).Entries);
            return retryPolicy.ExecuteAction(context.SaveChanges);
        }
    }

    public enum RefreshConflict
    {
        StoreWins,

        ClientWins,

        MergeClientAndStore
    }

    public static partial class DbContextExtensions
    {
        public static int SaveChanges(this DbContext context, RefreshConflict refreshMode, int retryCount = 3)
        {
            if (retryCount <= 0)
            {
                throw new ArgumentOutOfRangeException($"{retryCount} must be greater than 0.", nameof(retryCount));
            }

            return context.SaveChanges(
                conflicts => conflicts.ForEach(tracking => tracking.Refresh(refreshMode)), retryCount);
        }

        public static int SaveChanges(
            this DbContext context, RefreshConflict refreshMode, RetryStrategy retryStrategy) =>
                context.SaveChanges(
                    conflicts => conflicts.ForEach(tracking => tracking.Refresh(refreshMode)), retryStrategy);
    }

    public static partial class DbEntityEntryExtensions
    {
        public static EntityEntry Refresh(this EntityEntry tracking, RefreshConflict refreshMode)
        {
            switch (refreshMode)
            {
                case RefreshConflict.StoreWins:
                    {
                        // When entity is already deleted in database, Reload sets tracking state to Detached.
                        // When entity is already updated in database, Reload sets tracking state to Unchanged.
                        tracking.Reload(); // Execute SELECT.
                        // Hereafter, SaveChanges ignores this entity.
                        break;
                    }
                case RefreshConflict.ClientWins:
                    {
                        PropertyValues databaseValues = tracking.GetDatabaseValues(); // Execute SELECT.
                        if (databaseValues == null)
                        {
                            // When entity is already deleted in database, there is nothing for client to win against.
                            // Manually set tracking state to Detached.
                            tracking.State = EntityState.Detached;
                            // Hereafter, SaveChanges ignores this entity.
                        }
                        else
                        {
                            // When entity is already updated in database, refresh original values, which go to in WHERE clause.
                            tracking.OriginalValues.SetValues(databaseValues);
                            // Hereafter, SaveChanges executes UPDATE/DELETE for this entity, with refreshed values in WHERE clause.
                        }
                        break;
                    }
                case RefreshConflict.MergeClientAndStore:
                    {
                        PropertyValues databaseValues = tracking.GetDatabaseValues(); // Execute SELECT.
                        if (databaseValues == null)
                        {
                            // When entity is already deleted in database, there is nothing for client to merge with.
                            // Manually set tracking state to Detached.
                            tracking.State = EntityState.Detached;
                            // Hereafter, SaveChanges ignores this entity.
                        }
                        else
                        {
                            // When entity is already updated, refresh original values, which go to WHERE clause.
                            PropertyValues originalValues = tracking.OriginalValues.Clone();
                            tracking.OriginalValues.SetValues(databaseValues);
                            // If database has an different value for a property, then retain the database value.
#if NETFX
                            databaseValues.PropertyNames // Navigation properties are not included.
                                .Where(property => !object.Equals(originalValues[property], databaseValues[property]))
                                .ForEach(property => tracking.Property(property).IsModified = false);
#else
                            databaseValues.Properties // Navigation properties are not included.
                                    .Where(property => !object.Equals(originalValues[property.Name], databaseValues[property.Name]))
                                    .ForEach(property => tracking.Property(property.Name).IsModified = false);
#endif
                            // Hereafter, SaveChanges executes UPDATE/DELETE for this entity, with refreshed values in WHERE clause.
                        }
                        break;
                    }
            }
            return tracking;
        }
    }

    internal static partial class Concurrency
    {
        internal static void SaveChanges()
        {
            using (AdventureWorks adventureWorks1 = new AdventureWorks())
            using (AdventureWorks adventureWorks2 = new AdventureWorks())
            {
                const int id = 950;
                Product productCopy1 = adventureWorks1.Products.Find(id);
                Product productCopy2 = adventureWorks2.Products.Find(id);

                productCopy1.Name = nameof(adventureWorks1);
                productCopy1.ListPrice = 100;
                adventureWorks1.SaveChanges();

                productCopy2.Name = nameof(adventureWorks2);
                productCopy2.ProductSubcategoryID = 1;
                adventureWorks2.SaveChanges(RefreshConflict.MergeClientAndStore);
            }
        }
    }
}

#if DEMO
namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class ProductPhoto
    {
        [ConcurrencyCheck]
        public DateTime ModifiedDate { get; set; }
    }
}

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;

    public class DbUpdateException : DataException
    {
        public IEnumerable<DbEntityEntry> Entries { get; }
    }

    public class DbUpdateConcurrencyException : DbUpdateException
    {
    }
}
#endif