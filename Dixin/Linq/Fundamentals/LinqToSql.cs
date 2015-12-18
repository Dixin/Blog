namespace Dixin.Linq.Fundamentals
{
    using System.Linq;

    public static partial class LinqToSql
    {
        public static string[] ProductNames(string categoryName)
        {
            using (NorthwindDataContext database = new NorthwindDataContext())
            {
                IQueryable<string> query = from product in database.Products
                                           where product.Category.CategoryName == categoryName
                                           orderby product.UnitPrice ascending
                                           select product.ProductName; // Define query.
                return query.ToArray(); // Execute query.
            }
        }

        public static string[] ProductNames(string categoryName, int pageSize, int pageIndex)
        {
            using (NorthwindDataContext database = new NorthwindDataContext())
            {
                IQueryable<string> query = (from product in database.Products
                                            where product.Category.CategoryName == categoryName
                                            orderby product.ProductName
                                            select product.ProductName)
                    .Skip(pageSize * checked(pageIndex - 1))
                    .Take(pageSize); // Define query.
                return query.ToArray(); // Execute query.
            }
        }
    }

    public static partial class LinqToSql
    {
        public static string[] ProductNames2(string categoryName, int pageSize, int pageIndex)
        {
            using (NorthwindDataContext database = new NorthwindDataContext())
            {
                IQueryable<string> query = database.Products
                    .Where(product => product.Category.CategoryName == categoryName)
                    .OrderBy(product => product.ProductName)
                    .Select(product => product.ProductName)
                    .Skip(pageSize * checked(pageIndex - 1))
                    .Take(pageSize); // Define query.
                return query.ToArray(); // Execute query.
            }
        }
    }
}
