namespace Dixin.Linq.Introduction
{
#if NETFX
    using System.Diagnostics;
    using System.Linq;

    using Dixin.Linq.EntityFramework;

    using StockItem = Dixin.Linq.EntityFramework.StockItem;

    internal static partial class LinqToEntities
    {
        internal static void QueryExpression()
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                IQueryable<StockItem> source = adventureWorks.StockItems; // Get source.
                IQueryable<string> query = from product in source
                                           where product.Supplier.SupplierCategory.SupplierCategoryName == "Bikes"
                                           orderby product.UnitPrice
                                           select product.StockItemName; // Define query.
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }
    }

    internal static partial class LinqToEntities
    {
        internal static void QueryMethods()
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                IQueryable<StockItem> source = adventureWorks.StockItems; // Get source.
                IQueryable<string> query = source
                    .Where(product => product.Supplier.SupplierCategory.SupplierCategoryName == "Bikes")
                    .OrderBy(product => product.UnitPrice)
                    .Select(product => product.StockItemName); // Define query.
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }
    }

    internal static partial class LinqToEntities
    {
        internal static string[] ProductNames(string categoryName)
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                IQueryable<string> query =
                    from product in adventureWorks.StockItems
                    where product.Supplier.SupplierCategory.SupplierCategoryName == categoryName
                    orderby product.UnitPrice ascending
                    select product.StockItemName; // Define query.
                return query.ToArray(); // Execute query.
            }
        }
    }

    internal static partial class LinqToEntities
    {
        internal static string[] ProductNames(string categoryName, int pageSize, int pageIndex)
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                IQueryable<string> query =
                    (from product in adventureWorks.StockItems
                     where product.Supplier.SupplierCategory.SupplierCategoryName == categoryName
                     orderby product.UnitPrice ascending
                     select product.StockItemName)
                    .Skip(pageSize * checked(pageIndex - 1))
                    .Take(pageSize); // Define query.
                return query.ToArray(); // Execute query.
            }
        }
    }

    internal static partial class LinqToEntities
    {
        internal static string[] ProductNames2(string categoryName, int pageSize, int pageIndex)
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                IQueryable<string> query = adventureWorks
                    .StockItems
                    .Where(product => product.Supplier.SupplierCategory.SupplierCategoryName == categoryName)
                    .OrderBy(product => product.UnitPrice)
                    .Select(product => product.StockItemName)
                    .Skip(pageSize * checked(pageIndex - 1))
                    .Take(pageSize); // Define query.
                return query.ToArray(); // Execute query.
            }
        }
    }
#endif
}
