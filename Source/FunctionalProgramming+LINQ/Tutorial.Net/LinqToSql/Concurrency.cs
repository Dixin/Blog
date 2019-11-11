namespace Tutorial.LinqToSql
{
    using System;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Diagnostics;
    using System.Linq;
    using System.Transactions;

    internal partial class DbReaderWriter : IDisposable
    {
        private readonly DataContext context;

        internal DbReaderWriter(DataContext context)
        {
            this.context = context;
        }

        internal TEntity Read<TEntity>
            (params object[] keys) where TEntity : class => this.context.Find<TEntity>(keys);

        internal void Write(Action change)
        {
            change();
            this.context.SubmitChanges();
        }

        internal Table<TEntity> Set<TEntity>() where TEntity : class => this.context.GetTable<TEntity>();

        public void Dispose() => this.context.Dispose();
    }

    internal static partial class Concurrency
    {
        internal static void DefaultControl() // Check all columns, first client wins.
        {
            using (new TransactionScope()) // BEGIN TRANSACTION.
            using (DbReaderWriter readerWriter1 = new DbReaderWriter(new AdventureWorks()))
            using (DbReaderWriter readerWriter2 = new DbReaderWriter(new AdventureWorks()))
            using (DbReaderWriter readerWriter3 = new DbReaderWriter(new AdventureWorks()))
            {
                const int Id = 1;
                ProductCategory category1 = readerWriter1.Read<ProductCategory>(Id);
                ProductCategory category2 = readerWriter2.Read<ProductCategory>(Id);
                readerWriter1.Write(() => category1.Name = nameof(readerWriter1));
                try
                {
                    readerWriter2.Write(() => category2.Name = nameof(readerWriter2));
                }
                catch (ChangeConflictException exception)
                {
                    Trace.WriteLine(exception); // Row not found or changed.
                }

                Trace.WriteLine(readerWriter3.Read<ProductCategory>(Id).Name); // client1.
            } // ROLLBACK TRANSACTION.
        }
    }

#if DEMO
    public partial class ProductPhoto
    {
        [Column(DbType = "datetime NOT NULL", UpdateCheck = UpdateCheck.Always)]
        public DateTime ModifiedDate { get; set; }
    }
#endif

    internal static partial class Concurrency
    {
        internal static void CheckModifiedDate()
        {
            using (new TransactionScope()) // BEGIN TRANSACTION.
            using (DbReaderWriter readerWriter1 = new DbReaderWriter(new AdventureWorks()))
            using (DbReaderWriter readerWriter2 = new DbReaderWriter(new AdventureWorks()))
            {
                const int Id = 1;
                ProductPhoto photo1 = readerWriter1.Read<ProductPhoto>(Id);
                ProductPhoto photo2 = readerWriter2.Read<ProductPhoto>(Id);
                readerWriter1.Write(() =>
                {
                    photo1.LargePhotoFileName = nameof(readerWriter1);
                    photo1.ModifiedDate = DateTime.Now;
                });
                readerWriter2.Write(() =>
                {
                    photo2.LargePhotoFileName = nameof(readerWriter1);
                    photo2.ModifiedDate = DateTime.Now;
                }); // ChangeConflictException.
            } // ROLLBACK TRANSACTION.
        }
    }

    public partial class Product
    {
        [Column(AutoSync = AutoSync.Always, DbType = "rowversion NOT NULL",
            CanBeNull = false, IsDbGenerated = true, IsVersion = true, UpdateCheck = UpdateCheck.Never)]
        public Binary RowVersion { get; set; }
    }

    internal partial class DbReaderWriter
    {
        internal void Write
            (Action change, Action<ChangeConflictCollection> resolve, int retryCount = 3)
        {
            change();
            for (int retry = 1; retry < retryCount; retry++)
            {
                try
                {
                    this.context.SubmitChanges();
                    return;
                }
                catch (ChangeConflictException)
                {
                    resolve(this.context.ChangeConflicts);
                }
            }
            this.context.SubmitChanges();
        }
    }

    internal static partial class Concurrency
    {
        internal static void Conflict(Action<ChangeConflictCollection> resolve)
        {
            using (new TransactionScope()) // BEGIN TRANSACTION.
            using (DbReaderWriter readerWriter1 = new DbReaderWriter(new AdventureWorks()))
            using (DbReaderWriter readerWriter2 = new DbReaderWriter(new AdventureWorks()))
            using (DbReaderWriter readerWriter3 = new DbReaderWriter(new AdventureWorks()))
            using (DbReaderWriter readerWriter4 = new DbReaderWriter(new AdventureWorks()))
            {
                const int Id = 999;
                Product product1 = readerWriter1.Read<Product>(Id);
                Product product2 = readerWriter2.Read<Product>(Id);
                readerWriter1.Write(() => { product1.Name = nameof(readerWriter1); product1.ListPrice = 0; });
                Product product4 = readerWriter4.Read<Product>(Id);
                Trace.WriteLine($"({product4.Name}, {product4.ListPrice}, {product4.ProductSubcategoryID})");
                readerWriter2.Write(() => { product2.Name = nameof(readerWriter2); product2.ProductSubcategoryID = null; }, resolve);

                Product product3 = readerWriter3.Read<Product>(Id);
                Trace.WriteLine($"({product3.Name}, {product3.ListPrice}, {product3.ProductSubcategoryID})");
            } // ROLLBACK TRANSACTION.
        }

        internal static void DatabaseWins() => Conflict(conflicts => conflicts.ForEach(conflict =>
        {
            conflict.MemberConflicts.ForEach(member => Trace.WriteLine(
                $"{member.Member.Name}: client: {member.OriginalValue} -> {member.CurrentValue}, database: {member.DatabaseValue}"));
            conflict.Resolve(RefreshMode.OverwriteCurrentValues);
        }));
        // RowVersion: client: "AAAAAAAACAM=" -> "AAAAAAAACAM=", database: "AAAAAAADQ/Y="
        // Name: client: Road-750 Black, 52 -> client2, database: client1
        // ListPrice: client: 539.9900 -> 539.9900, database: 0.0000
        // (client1, 0.0000, 2)

        internal static void ClientWins() => Conflict(conflicts => conflicts.ForEach(conflict =>
        {
            conflict.MemberConflicts.ForEach(member => Trace.WriteLine(
                $"{member.Member.Name}: client: {member.OriginalValue} -> {member.CurrentValue}, database: {member.DatabaseValue}"));
            conflict.Resolve(RefreshMode.KeepCurrentValues);
        }));
        // RowVersion: client: "AAAAAAAACAM=" -> "AAAAAAAACAM=", database: "AAAAAAADQ/c="
        // Name: client: Road-750 Black, 52 -> client2, database: client1
        // ListPrice: client: 539.9900 -> 539.9900, database: 0.0000
        // (client2, 539.9900, )

        internal static void MergeClientAndDatabase() => Conflict(conflicts => conflicts.ForEach(conflict =>
        {
            conflict.MemberConflicts.ForEach(member => Trace.WriteLine(
                $"{member.Member.Name}: client: {member.OriginalValue} -> {member.CurrentValue}, database: {member.DatabaseValue}"));
            conflict.Resolve(RefreshMode.KeepChanges);
        }));
        // RowVersion: client: "AAAAAAAACAM=" -> "AAAAAAAACAM=", database: "AAAAAAADQ/k="
        // Name: client: Road-750 Black, 52 -> client2, database: client1
        // ListPrice: client: 539.9900 -> 539.9900, database: 0.0000
        // (client2, 0.0000, )
    }
}
