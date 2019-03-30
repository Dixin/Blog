namespace Tutorial.LinqToEntities
{
#if EF
    using System;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.SqlServer;
    using System.Runtime.Remoting.Messaging;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;

    using ModelBuilder = System.Data.Entity.DbModelBuilder;
    using DatabaseFacade = System.Data.Entity.Database;
    using ChangeTracker = System.Data.Entity.Infrastructure.DbChangeTracker;
    using EntityEntry = System.Data.Entity.Infrastructure.DbEntityEntry;
    using PropertyEntry = System.Data.Entity.Infrastructure.DbPropertyEntry;
    using IDbContextTransaction = System.Data.Entity.DbContextTransaction;
#else
    using System;
    using System.Data.Common;
    using System.Data.SqlClient;

    using Microsoft.EntityFrameworkCore;
#endif

#if EF
    public partial class AdventureWorks : DbContext { }

    public partial class AdventureWorks
    {
        public AdventureWorks(DbConnection connection = null) : base(
            existingConnection: connection ?? new SqlConnection(ConnectionStrings.AdventureWorks),
            contextOwnsConnection: connection == null)
        { }
    }

    public class RetryConfiguration : DbConfiguration
    {
        public RetryConfiguration()
        {
            this.SetExecutionStrategy(
                providerInvariantName: SqlProviderServices.ProviderInvariantName,
                getExecutionStrategy: () => ExecutionStrategy.DisableExecutionStrategy
                    ? new DefaultExecutionStrategy() : ExecutionStrategy.Create());
        }
    }

    public partial class ExecutionStrategy
    {

        public static bool DisableExecutionStrategy
        {
            get => (bool?)CallContext.LogicalGetData(nameof(DisableExecutionStrategy)) ?? false;
            set => CallContext.LogicalSetData(nameof(DisableExecutionStrategy), value);
        }

        public static IDbExecutionStrategy Create() =>
            new SqlAzureExecutionStrategy(maxRetryCount: 5, maxDelay: TimeSpan.FromSeconds(30));
    }

    public partial class ExecutionStrategy : IDbExecutionStrategy
    {
        private readonly IDbExecutionStrategy strategy = Create();

        public bool RetriesOnFailure => this.strategy.RetriesOnFailure;

        public void Execute(Action operation) =>
            ExecuteOperation(() => { this.strategy.Execute(operation); return (object)null; });

        public TResult Execute<TResult>(Func<TResult> operation) =>
            ExecuteOperation(() => this.strategy.Execute(operation));

        public Task ExecuteAsync(
            Func<Task> operation, CancellationToken cancellationToken = default) =>
                ExecuteOperation(() => this.strategy.ExecuteAsync(operation, cancellationToken));

        public Task<TResult> ExecuteAsync<TResult>(
            Func<Task<TResult>> operation, CancellationToken cancellationToken = default) =>
                ExecuteOperation(() => this.strategy.ExecuteAsync(operation, cancellationToken));

        private static T ExecuteOperation<T>(Func<T> resultFactory)
        {
            DisableExecutionStrategy = true;
            try
            {
                return resultFactory();
            }
            finally
            {
                DisableExecutionStrategy = false;
            }
        }
    }

    public static class DatabaseExtensions
    {
        public static ExecutionStrategy CreateExecutionStrategy(this DatabaseFacade database) => 
            new ExecutionStrategy();
    }
#else
    public partial class AdventureWorks : DbContext { }

    public partial class AdventureWorks
    {
        public AdventureWorks(DbConnection connection = null)
            : base(new DbContextOptionsBuilder<AdventureWorks>().UseSqlServer(
                connection: connection ?? new SqlConnection(ConnectionStrings.AdventureWorks),
                sqlServerOptionsAction: options => options.EnableRetryOnFailure(
                    maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null)).Options) { }
    }
#endif

    public partial class AdventureWorks
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            MapCompositePrimaryKey(modelBuilder);
            MapManyToMany(modelBuilder);
            MapDiscriminator(modelBuilder);
        }
    }

    internal static partial class UnitOfWork
    {
        internal static void Dispose()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                // Unit of work.
            }
        }
    }

#if EF
    public partial class AdventureWorks
    {
        static AdventureWorks() =>
            Database.SetInitializer(new NullDatabaseInitializer<AdventureWorks>());
    }
#endif

#if DEMO
    public class SqlConfiguration : DbConfiguration
    {
        public SqlConfiguration() =>
             this.SetManifestTokenResolver(new SqlManifestTokenResolver());
    }

    public class SqlManifestTokenResolver : IManifestTokenResolver
    {
        public string ResolveManifestToken(DbConnection connection) => "2012";
    }
#endif
}

#if DEMO
namespace Microsoft.EntityFrameworkCore
{
    using System;

    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public class DbContext : IDisposable, IInfrastructure<IServiceProvider>
    {
        public DbContext(DbContextOptions options);

        public virtual ChangeTracker ChangeTracker { get; }

        public virtual DatabaseFacade Database { get; }

        public virtual void Dispose();

        public virtual int SaveChanges();

        public virtual DbSet<TEntity> Set<TEntity>() where TEntity : class;

        protected internal virtual void OnModelCreating(ModelBuilder modelBuilder);

        // Other members.
    }
}

namespace System.Data.Entity
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;

    public class DbContext : IDisposable, IObjectContextAdapter
    {
        public DbContext(DbConnection existingConnection, bool contextOwnsConnection);

        public DbChangeTracker ChangeTracker { get; }

        public Database Database { get; }

        public void Dispose();

        public virtual int SaveChanges();

        public virtual DbSet<TEntity> Set<TEntity>() where TEntity : class;

        protected virtual void OnModelCreating(DbModelBuilder modelBuilder);

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

    internal interface IInternalQueryAdapter { }

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

    internal interface IInternalSetAdapter { }

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
        public virtual void InitializeDatabase(TContext context) { }
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
