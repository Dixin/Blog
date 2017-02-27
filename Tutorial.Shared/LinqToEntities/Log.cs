namespace Tutorial.LinqToEntities
{
#if EF
    using System;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Linq;

    using static Tutorial.LinqToObjects.EnumerableX;

    internal static partial class Log
    {
        internal static void DbQueryToString()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductCategory> source = adventureWorks.ProductCategories; // Define query.
                source.ToString().WriteLine();
                // SELECT 
                //    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
                //    [Extent1].[Name] AS [Name]
                //    FROM [Production].[ProductCategory] AS [Extent1]
                source.ForEach(); // Execute query.
            }
        }
    }

    internal static partial class Log
    {
        internal static void DatabaseLog()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.Database.Log = log => Trace.Write(log);
                IQueryable<ProductCategory> source = adventureWorks.ProductCategories; // Define query.
                source.ForEach(); // Execute query.
            }
            // Opened connection at 5/21/2016 12:33:34 AM -07:00
            // SELECT 
            //    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            //    [Extent1].[Name] AS [Name]
            //    FROM [Production].[ProductCategory] AS [Extent1]
            // -- Executing at 5/21/2016 12:31:58 AM -07:00
            // -- Completed in 11 ms with result: SqlDataReader4
            // Closed connection at 5/21/2016 12:33:35 AM -07:00
        }
    }

    public class DbCommandInterceptor : IDbCommandInterceptor
    {
        private readonly Action<string> logger;

        internal DbCommandInterceptor(Action<string> logger)
        {
            this.logger = logger;
        }

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext) =>
            this.Log(nameof(this.NonQueryExecuting), interceptionContext, command);

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext) =>
            this.Log(nameof(this.NonQueryExecuting), interceptionContext);

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext) =>
            this.Log(nameof(this.ReaderExecuting), interceptionContext, command);

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext) =>
            this.Log(nameof(this.ReaderExecuted), interceptionContext);

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext) =>
            this.Log(nameof(this.ScalarExecuting), interceptionContext, command);

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext) =>
            this.Log(nameof(this.ScalarExecuted), interceptionContext);

        private void Log<TResult>(
            string @event, DbCommandInterceptionContext<TResult> interceptionContext, DbCommand command = null)
        {
            Exception exception = interceptionContext.Exception;
            if (exception != null)
            {
                this.logger($"{@event}: {exception}");
            }
            if (command == null)
            {
                this.logger(@event);
            }
            else
            {
                this.logger($@"{@event}: {command.CommandText}{string.Concat(command.Parameters
                    .Cast<DbParameter>()
                    .Select(parameter => $", {parameter.ParameterName}={parameter.Value}"))}");
            }
        }
    }

    internal static partial class Log
    {
        internal static void DbCommandInterceptor()
        {
            IDbCommandInterceptor dbCommandTrace = new DatabaseLogFormatter(writeAction: log => Trace.WriteLine(log));
            DbInterception.Add(dbCommandTrace);
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductCategory> source = adventureWorks.ProductCategories; // Define query.
                source.ForEach(); // Execute query.
            }
            // Opened connection at 1/11/2017 11:51:06 PM -08:00
            // SELECT 
            // [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            // [Extent1].[Name] AS [Name]
            // FROM [Production].[ProductCategory] AS [Extent1]
            // -- Executing at 1/11/2017 11:51:06 PM -08:00
            // -- Completed in 10 ms with result: SqlDataReader
            // Closed connection at 1/11/2017 11:51:06 PM -08:00
            DbInterception.Remove(dbCommandTrace);
        }
    }
#else
    using System;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using static Tutorial.LinqToObjects.EnumerableX;
#endif

#if !EF
    public class TraceLogger : ILogger
    {
        private readonly string categoryName;

        public TraceLogger(string categoryName) => this.categoryName = categoryName;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            Trace.WriteLine($"{DateTime.Now.ToString("o")} {logLevel} {eventId.Id} {this.categoryName}");
            Trace.WriteLine(formatter(state, exception));
        }

        public IDisposable BeginScope<TState>(TState state) => null;
    }

    public class TraceLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new TraceLogger(categoryName);

        public void Dispose() { }
    }
#endif

#if !EF
    public partial class AdventureWorks
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            LoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new TraceLoggerProvider());
            optionsBuilder.UseLoggerFactory(loggerFactory);
        }
    }
#endif

    internal static partial class Log
    {
        internal static void TraceLogger()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductCategory> source = adventureWorks.ProductCategories; // Define query.
                source.ForEach(); // Execute query.
            }
            // 2017-01-11T22:15:43.4625876-08:00 Debug 2 Microsoft.EntityFrameworkCore.Query.Internal.SqlServerQueryCompilationContextFactory
            // Compiling query model: 
            // 'from ProductCategory <generated>_0 in DbSet<ProductCategory>
            // select <generated>_0'

            // 2017-01-11T22:15:43.4932882-08:00 Debug 3 Microsoft.EntityFrameworkCore.Query.Internal.SqlServerQueryCompilationContextFactory
            // Optimized query model: 
            // 'from ProductCategory <generated>_0 in DbSet<ProductCategory>
            // select <generated>_0'

            // 2017-01-11T22:15:43.6179834-08:00 Debug 5 Microsoft.EntityFrameworkCore.Query.Internal.SqlServerQueryCompilationContextFactory
            // TRACKED: True
            // (QueryContext queryContext) => IEnumerable<ProductCategory> _ShapedQuery(
            //    queryContext: queryContext, 
            //    shaperCommandContext: SelectExpression: 
            //        SELECT [p].[ProductCategoryID], [p].[Name]
            //        FROM [Production].[ProductCategory] AS [p]
            //    , 
            //    shaper: UnbufferedEntityShaper<ProductCategory>
            // )

            // 2017-01-11T22:15:43.7272876-08:00 Debug 3 Microsoft.EntityFrameworkCore.Storage.Internal.SqlServerConnection
            // Opening connection to database 'AdventureWorks' on server 'tcp:tutorialsql.database.windows.net,1433'.

            // 2017-01-11T22:15:44.1024201-08:00 Information 1 Microsoft.EntityFrameworkCore.Storage.IRelationalCommandBuilderFactory
            // Executed DbCommand (66ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
            // SELECT [p].[ProductCategoryID], [p].[Name]
            // FROM [Production].[ProductCategory] AS [p]

            // 2017-01-11T22:15:44.1505353-08:00 Debug 4 Microsoft.EntityFrameworkCore.Storage.Internal.SqlServerConnection
            // Closing connection to database 'AdventureWorks' on server 'tcp:tutorialsql.database.windows.net,1433'.
        }
    }
}

#if DEMO
namespace System.Data.Entity
{
    using System.Data.Entity.Infrastructure;

    public class DbContext : IDisposable, IObjectContextAdapter
    {
        public Database Database { get; }

        // Other members.
    }

    public class Database
    {
        public Action<string> Log { get; set; }

        // Other members.
    }
}

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Common;

    public interface IDbCommandInterceptor : IDbInterceptor // IDbInterceptor is an empty interface.
    {
        void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext);

        void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext);

        void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext);

        void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext);

        void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext);

        void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext);
    }
}
#endif
