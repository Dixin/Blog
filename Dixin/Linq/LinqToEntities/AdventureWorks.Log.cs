namespace Dixin.Linq.LinqToEntities
{
    using System;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Diagnostics;
    using System.Linq;

    public class CustomDbCommandInterceptor : IDbCommandInterceptor
    {
        private readonly Action<DbCommand> begin;

        private readonly Action end;

        public CustomDbCommandInterceptor(Action<string> write)
        {
            this.begin = command => write?.Invoke($@"{command.CommandText}{string.Concat(command.Parameters
                .OfType<DbParameter>()
                .Select(parameter => $", {parameter.ParameterName}={parameter.Value}"))}");
            this.end = () => write?.Invoke("Done.");
        }

        public void NonQueryExecuting
            (DbCommand command, DbCommandInterceptionContext<int> _) => this.begin(command);

        public void NonQueryExecuted
            (DbCommand command, DbCommandInterceptionContext<int> _) => this.end();

        public void ReaderExecuting
            (DbCommand command, DbCommandInterceptionContext<DbDataReader> _) => this.begin(command);

        public void ReaderExecuted
            (DbCommand command, DbCommandInterceptionContext<DbDataReader> _) => this.end();

        public void ScalarExecuting
            (DbCommand command, DbCommandInterceptionContext<object> _) => this.begin(command);

        public void ScalarExecuted
            (DbCommand command, DbCommandInterceptionContext<object> _) => this.end();
    }

    public static partial class Log
    {
        static Log()
        {
            // Log with CustomDbCommandInterceptor.
            DbInterception.Add(new CustomDbCommandInterceptor(log => Trace.WriteLine(log)));
            // select cast(serverproperty('EngineEdition') as int)
            // Done.
            // SELECT 
            //    CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
            //    [Extent1].[ProductID] AS [ProductID], 
            //    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
            //    [Extent1].[Name] AS [Name], 
            //    [Extent1].[ListPrice] AS [ListPrice]
            //    FROM [Production].[Product] AS [Extent1]
            //    WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))
            // Done.
        }

        public static void WhereWithLog()
        {
            using (AdventureWorksDbContext adventureWorks = new AdventureWorksDbContext())
            {
                IQueryable<Product> source = adventureWorks.Products;
                IQueryable<Product> products = source.Where(product => product.ListPrice > 100); // Define query.

                // Log with DbQuery<T>.ToString.
                Trace.WriteLine(products.ToString());
                //  SELECT 
                //    CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
                //    [Extent1].[ProductID] AS [ProductID], 
                //    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
                //    [Extent1].[Name] AS [Name], 
                //    [Extent1].[ListPrice] AS [ListPrice]
                //    FROM [Production].[Product] AS [Extent1]
                //    WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))

                // Log with Database.Log.
                adventureWorks.Database.Log = log => Trace.Write(log);
                // Opened connection at 1/5/2016 9:22:18 AM -08:00
                // SELECT 
                //    CASE WHEN (((CASE WHEN ([Extent1].[Style] = N'M ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'U ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1) AND ((CASE WHEN ([Extent1].[Style] = N'W ') THEN cast(1 as bit) ELSE cast(0 as bit) END) <> 1)) THEN '0X' WHEN ([Extent1].[Style] = N'M ') THEN '0X0X' WHEN ([Extent1].[Style] = N'U ') THEN '0X1X' ELSE '0X2X' END AS [C1], 
                //    [Extent1].[ProductID] AS [ProductID], 
                //    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
                //    [Extent1].[Name] AS [Name], 
                //    [Extent1].[ListPrice] AS [ListPrice]
                //    FROM [Production].[Product] AS [Extent1]
                //    WHERE [Extent1].[ListPrice] > cast(100 as decimal(18))
                // -- Executing at 1/5/2016 9:22:20 AM -08:00
                // -- Completed in 8 ms with result: SqlDataReader

                products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
            }
        }
    }
}
