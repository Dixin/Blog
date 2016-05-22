namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Diagnostics;
    using System.Linq;

    internal static partial class Log
    {
        internal static void ToString()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductCategory> source = adventureWorks.ProductCategories; // Define query.
                string translatedSql = source.ToString();
                Trace.WriteLine(translatedSql);
                // SELECT 
                //    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
                //    [Extent1].[Name] AS [Name]
                //    FROM [Production].[ProductCategory] AS [Extent1]
                source.ForEach(category => Trace.WriteLine(category.Name)); // Execute query.
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
                source.ForEach(category => Trace.WriteLine(category.Name)); // Execute query.
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

    internal partial class DbCommandInterceptor : IDbCommandInterceptor
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
            if (command != null)
            {
                this.log($@"{@event}: {command.CommandText}{string.Concat(command.Parameters
                    .OfType<DbParameter>()
                    .Select(parameter => $", {parameter.ParameterName}={parameter.Value}"))}");
            }
            else
            {
                this.log(interceptionContext.Exception != null ? $"{@event}: {interceptionContext.Exception}" : @event);
            }
        }
    }

    internal partial class DbCommandInterceptor
    {
        internal static void Register(Action<string> log = null) => 
            DbInterception.Add(new DbCommandInterceptor(log ?? (message => Trace.WriteLine(message))));
    }

    internal static partial class Log
    {
        internal static void DbInterception()
        {
            DbCommandInterceptor.Register();
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductCategory> source = adventureWorks.ProductCategories; // Define query.
                source.ForEach(category => Trace.WriteLine(category.Name)); // Execute query.
                // DbCommandInterceptor: SELECT 
                //    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
                //    [Extent1].[Name] AS [Name]
                //    FROM [Production].[ProductCategory] AS [Extent1]
                // DbCommandInterceptor: Done.
            }
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
