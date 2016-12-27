namespace Dixin.Linq.EntityFramework
{
    using System.Data.Common;
#if NETFX
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
#else
    using System.Data.SqlClient;
#endif
    using System.Diagnostics;
    using System.Linq;

#if !NETFX
    using Microsoft.EntityFrameworkCore;
#endif

    public partial class AdventureWorks : DbContext
    {
#if NETFX
        public AdventureWorks()
            : base(ConnectionStrings.AdventureWorks)
        {
        }
#else
        public AdventureWorks(DbConnection connection = null)
            : base(new DbContextOptionsBuilder<AdventureWorks>().UseSqlServer(
                connection ?? new SqlConnection(ConnectionStrings.AdventureWorks)).Options)
        {
        }
#endif
    }

    internal static partial class Query
    {
        internal static void Dispose()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                // Unit of work.
            }
        }
    }

    internal static partial class Query
    {
        internal static void Table()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductCategory> allRowsInTable = adventureWorks.ProductCategories;
                allRowsInTable.ForEach(categoryRow => Trace.WriteLine(
                    $"{categoryRow.ProductCategoryID}:{categoryRow.Name}"));
                // 1:Bikes 2:Components 3:Clothing 4:Accessories 
            }
        }
    }

#if NETFX
    public partial class AdventureWorks
    {
        static AdventureWorks()
        {
            Database.SetInitializer(new NullDatabaseInitializer<AdventureWorks>()); // Call once.
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
