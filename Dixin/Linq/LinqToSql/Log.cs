namespace Dixin.Linq.LinqToSql
{
    using System.Data.Common;
    using System.Diagnostics;
    using System.Linq;

    using Dixin.IO;

    internal static partial class Log
    {
        internal static void DataQueryToString()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductCategory> source = adventureWorks.ProductCategories; // Define query.
                string translatedSql = source.ToString();
                Trace.WriteLine(translatedSql);
                // SELECT[t0].[ProductID], [t0].[Name], [t0].[ListPrice], [t0].[ProductSubcategoryID]
                // FROM[Production].[Product]
                // AS[t0]
                // WHERE[t0].[ListPrice] > @p0
                source.ForEach(category => Trace.WriteLine(category.Name)); // Execute query.
            }
        }
        internal static void DataContextLog()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.Log = new CustomTextWriter(log => Trace.Write(log));
                IQueryable<ProductCategory> source = adventureWorks.ProductCategories; // Define query.
                source.ForEach(category => Trace.WriteLine(category.Name)); // Execute query.
                // TODO.
            }
        }

        internal static void DataContexGetCommand()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductCategory> source = adventureWorks.ProductCategories; // Define query.
                DbCommand command = adventureWorks.GetCommand(source);
                Trace.WriteLine($@"{command.CommandText}{string.Concat(command.Parameters
                    .Cast<DbParameter>()
                    .Select(parameter => $", {parameter.ParameterName}={parameter.Value}"))}");
                // TODO.
            }
        }
    }
}
