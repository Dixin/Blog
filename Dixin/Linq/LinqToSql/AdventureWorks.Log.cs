namespace Dixin.Linq.LinqToSql
{
    using System.Data.Common;
    using System.Diagnostics;
    using System.Linq;

    using Dixin.IO;

    public static partial class Log
    {

        public static void WhereWithLog()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<Product> source = adventureWorks.Products;
                IQueryable<Product> products = source.Where(product => product.ListPrice > 100); // Define query.

                // Log with DataQuery<T>.ToString.
                Trace.WriteLine(products.ToString());
                // SELECT[t0].[ProductID], [t0].[Name], [t0].[ListPrice], [t0].[ProductSubcategoryID]
                // FROM[Production].[Product]
                // AS[t0]
                // WHERE[t0].[ListPrice] > @p0

                // Log with DataContext.GetCommand.
                DbCommand command = adventureWorks.GetCommand(products);
                Trace.WriteLine($@"{command.CommandText}{string.Concat(command.Parameters
                    .OfType<DbParameter>()
                    .Select(parameter => $", {parameter.ParameterName}={parameter.Value}"))}");
                // SELECT[t0].[ProductID], [t0].[Name], [t0].[ListPrice], [t0].[ProductSubcategoryID]
                // FROM[Production].[Product]
                // AS[t0]
                // WHERE[t0].[ListPrice] > @p0, @p0=100

                // Log with DataContext.Log.
                adventureWorks.Log = new CustomTextWriter(log => Trace.Write(log));
                // SELECT [t0].[ProductID], [t0].[Name], [t0].[ListPrice], [t0].[ProductSubcategoryID]
                // FROM [Production].[Product] AS [t0]
                // WHERE [t0].[ListPrice] > @p0, @p0=100
                // SELECT [t0].[ProductID], [t0].[Name], [t0].[ListPrice], [t0].[ProductSubcategoryID]
                // FROM [Production].[Product] AS [t0]
                // WHERE [t0].[ListPrice] > @p0
                // -- @p0: Input Decimal (Size = -1; Prec = 33; Scale = 4) [100]
                // -- Context: SqlProvider(Sql2008) Model: AttributedMetaModel Build: 4.6.1038.0

                products.ForEach(product => Trace.WriteLine($"{product.Name}: {product.ListPrice}")); // Execute query.
            }
        }

    }
}
