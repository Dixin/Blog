namespace Tutorial.Introduction
{
#if NETFX
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
#endif

#if NETFX
    internal static partial class LinqToDataSets
    {
        internal static void QueryMethods()
        {
            using (DataSet dataSet = new DataSet())
            using (DataAdapter dataAdapter = new SqlDataAdapter(
                @"SELECT [Name], [ListPrice], [ProductSubcategoryID] FROM [Production].[Product]",
                @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AdventureWorks_Data.mdf;Integrated Security=True;Connect Timeout=30"))
            {
                dataAdapter.Fill(dataSet);
                EnumerableRowCollection<DataRow> source = dataSet.Tables[0].AsEnumerable(); // Get source.
                EnumerableRowCollection<string> query = source
                    .Where(product => product.Field<int>("ProductSubcategoryID") == 1)
                    .OrderBy(product => product.Field<decimal>("ListPrice"))
                    .Select(product => product.Field<string>("Name")); // Define query.
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }
    }
#endif

#if NETFX
    internal static partial class LinqToDataSets
    {
        internal static void QueryExpression()
        {
            using (DataSet dataSet = new DataSet())
            using (DataAdapter dataAdapter = new SqlDataAdapter(
                @"SELECT [Name], [ListPrice], [ProductSubcategoryID] FROM [Production].[Product]",
                @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AdventureWorks_Data.mdf;Integrated Security=True;Connect Timeout=30"))
            {
                dataAdapter.Fill(dataSet);
                EnumerableRowCollection<DataRow> source = dataSet.Tables[0].AsEnumerable(); // Get source.
                EnumerableRowCollection<string> query = from product in source
                    where product.Field<int>("ProductSubcategoryID") == 1
                    orderby product.Field<decimal>("ListPrice")
                    select product.Field<string>("Name"); // Define query.
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }
    }
#endif
}
