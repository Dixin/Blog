namespace Dixin.Linq.EntityFramework
{
#if !NETFX
    using System;
#endif
    using System.Data.Common;
#if NETFX
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
#endif
    using System.Data.SqlClient;
    using System.Linq;

#if !NETFX
    using Microsoft.EntityFrameworkCore;
#endif

#if NETFX
    public partial class WideWorldImporters : DbContext
    {
        public WideWorldImporters(DbConnection connection = null)
            : base(
                  existingConnection: connection ?? new SqlConnection(ConnectionStrings.AdventureWorks),
                  contextOwnsConnection: connection == null)
        {
        }
    }
#else
    public partial class WideWorldImporters : DbContext
    {
        public WideWorldImporters(DbConnection connection = null)
            : base(new DbContextOptionsBuilder<WideWorldImporters>().UseSqlServer(
                connection ?? new SqlConnection(ConnectionStrings.AdventureWorks),
                option => option.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null)).Options)
        {
        }
    }
#endif

#if DEMO
    public class RetryConfiguration : DbConfiguration
    {
        public RetryConfiguration()
        {
            this.SetExecutionStrategy(
                providerInvariantName: "System.Data.SqlClient",
                getExecutionStrategy: () => new SqlAzureExecutionStrategy(maxRetryCount: 5, maxDelay: TimeSpan.FromSeconds(30)));
        }
    }
#endif

    internal static partial class Query
    {
        internal static void Dispose()
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                // Unit of work.
            }
        }
    }

    internal static partial class Query
    {
        internal static void Table()
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                IQueryable<SupplierCategory> allRowsInTable = adventureWorks.SupplierCategories;
                allRowsInTable.WriteLines(categoryRow => $"{categoryRow.SupplierCategoryID}:{categoryRow.SupplierCategoryName}");
                // 1:Bikes 2:Components 3:Clothing 4:Accessories 
            }
        }
    }

#if NETFX
    public partial class WideWorldImporters
    {
        static WideWorldImporters()
        {
            Database.SetInitializer(new NullDatabaseInitializer<WideWorldImporters>()); // Call once.
            // Equivalent to: Database.SetInitializer<AdventureWorks>(null);
        }
    }

    public class SqlConfiguration : DbConfiguration
    {
        public SqlConfiguration()
        {
            this.SetManifestTokenResolver(new SqlManifestTokenResolver());
        }
    }

    public class SqlManifestTokenResolver : IManifestTokenResolver
    {
        public string ResolveManifestToken(DbConnection connection) => "2012";
    }
#endif
}

#if DEMO
namespace System.Data.Entity
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;

    public class DbContext : IDisposable, IObjectContextAdapter
    {
        public DbContext(string nameOrConnectionString);

        public DbChangeTracker ChangeTracker { get; }

        public DbContextConfiguration Configuration { get; }

        public Database Database { get; }

        ObjectContext IObjectContextAdapter.ObjectContext { get; } // From IObjectContextAdapter.

        public void Dispose(); // From IDisposable.

        // Other members.
    }
}

namespace System.Data.Entity.Infrastructure
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;

    internal interface IInternalQueryAdapter
    {
    }

    public class DbQuery<TResult> : IOrderedQueryable<TResult>, IQueryable<TResult>,
        IOrderedQueryable, IQueryable, IEnumerable<TResult>, IEnumerable,
        IDbAsyncEnumerable<TResult>, IDbAsyncEnumerable, IListSource, IInternalQueryAdapter
    {
        Type IQueryable.ElementType { get; }

        Expression IQueryable.Expression { get; }

        IQueryProvider IQueryable.Provider { get; } // Return System.Data.Entity.Internal.Linq.DbQueryProvider object.

        // Other members.
    }
}

namespace System.Data.Entity
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Linq;

    internal interface IInternalSetAdapter
    {
    }

    public class DbSet<TEntity> : DbQuery<TEntity>, IDbSet<TEntity>, IQueryable<TEntity>, IQueryable,
        IEnumerable<TEntity>, IEnumerable, IInternalSetAdapter where TEntity : class
    {
        // Members.
    }
}

namespace System.Data.Entity
{
    public interface IDatabaseInitializer<in TContext> where TContext : DbContext
    {
        void InitializeDatabase(TContext context);
    }
}

namespace System.Data.Entity
{
    public class NullDatabaseInitializer<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
    {
        public virtual void InitializeDatabase(TContext context)
        {
        }
    }
}

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Core.Common;

    internal class SqlProviderManifest : DbXmlEnabledProviderManifest
    {
        internal const string TokenSql8 = "2000";

        internal const string TokenSql9 = "2005";

        internal const string TokenSql10 = "2008";

        internal const string TokenSql11 = "2012";

        internal const string TokenAzure11 = "2012.Azure";

        // Other members.
    }
}
#endif
