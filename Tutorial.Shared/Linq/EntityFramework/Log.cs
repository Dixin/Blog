namespace Dixin.Linq.EntityFramework
{
#if NETFX
    using System;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Linq;

    internal static partial class Log
    {
        internal static void DbQueryToString()
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories; // Define query.
                string translatedSql = source.ToString();
                translatedSql.WriteLine();
                // SELECT 
                //    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
                //    [Extent1].[Name] AS [Name]
                //    FROM [Production].[ProductCategory] AS [Extent1]
                source.WriteLines(category => category.SupplierCategoryName); // Execute query.
            }
        }
    }

    internal static partial class Log
    {
        internal static void DatabaseLog()
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                adventureWorks.Database.Log = log => log.Write();
                IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories; // Define query.
                source.WriteLines(category => category.SupplierCategoryName); // Execute query.
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
    }

    internal class DbCommandInterceptor : IDbCommandInterceptor
    {
        private readonly Action<string> log;

        internal DbCommandInterceptor(Action<string> log)
        {
            this.log = log;
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
            if (command == null)
            {
                this.log(exception == null ? @event : $"{@event}: {exception}");
            }
            else
            {
                this.log($@"{@event}: {command.CommandText}{string.Concat(command.Parameters
                    .Cast<DbParameter>()
                    .Select(parameter => $", {parameter.ParameterName}={parameter.Value}"))}");
                if (exception != null)
                {
                    this.log($@"{@event}: {exception}");
                }
            }
        }
    }

    internal static partial class Log
    {
        internal static void DbCommandInterceptor()
        {
            DbCommandInterceptor dbCommandTrace = new DbCommandInterceptor(message => message.WriteLine());
            DbInterception.Add(dbCommandTrace);
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories; // Define query.
                source.WriteLines(category => category.SupplierCategoryName); // Execute query.
                // ReaderExecuting: SELECT 
                //    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
                //    [Extent1].[Name] AS [Name]
                //    FROM [Production].[ProductCategory] AS [Extent1]
                // ReaderExecuted
            }
            DbInterception.Remove(dbCommandTrace);
        }
    }
#else
    using System;
    using System.Linq;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    internal class TraceLogger : ILogger
    {
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            $"{logLevel} [{eventId.Id}] {formatter(state, exception)}".WriteLine();

        public IDisposable BeginScope<TState>(TState state) => null;
    }

    public class TraceLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new TraceLogger();

        public void Dispose() { }
    }

    public partial class WideWorldImporters
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            LoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new TraceLoggerProvider());
            optionsBuilder.UseLoggerFactory(loggerFactory);
        }
    }

    internal static partial class Log
    {
        internal static void TraceLogger()
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                IQueryable<SupplierCategory> source = adventureWorks.SupplierCategories; // Define query.
                source.WriteLines(category => category.SupplierCategoryName); // Execute query.
            }
            // Debug [2] Compiling query model: 
            // 'from ProductCategory <generated>_0 in DbSet<ProductCategory>
            // select <generated>_0'
            // Debug [3] Optimized query model: 
            // 'from ProductCategory <generated>_0 in DbSet<ProductCategory>
            // select <generated>_0'
            // Debug [5] TRACKED: True
            // (QueryContext queryContext) => IEnumerable<ProductCategory> _ShapedQuery(
            //    queryContext: queryContext, 
            //    shaperCommandContext: SelectExpression: 
            //        SELECT [p].[ProductCategoryID], [p].[Name]
            //        FROM [Production].[ProductCategory] AS [p]
            //    , 
            //    shaper: UnbufferedEntityShaper<ProductCategory>
            //)
            // Debug [3] Opening connection to database '' on server '(LocalDB)\MSSQLLocalDB'.
            // Information[1]Executed DbCommand (215ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
            // SELECT [p].[ProductCategoryID], [p].[Name]
            // FROM [Production].[ProductCategory] AS [p]
            // Debug [4] Closing connection to database 'C:\USERS\DIXIN\DOCUMENTS\GITHUB\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF' on server '(LocalDB)\MSSQLLocalDB'.
        }
    }
#endif
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
