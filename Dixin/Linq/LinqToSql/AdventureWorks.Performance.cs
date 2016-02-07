namespace Dixin.Linq.LinqToSql
{
    using System;
    using System.Data.Linq;
    using System.Linq;

    internal static class CompiledQueries
    {
        private static readonly Func<AdventureWorks, decimal, IQueryable<string>> GetProductNamesCompiled =
            CompiledQuery.Compile((AdventureWorks adventureWorks, decimal maxListPrice) => adventureWorks
                .Products
                .Where(product => product.ListPrice <= maxListPrice)
                .Select(product => product.Name));

        internal static IQueryable<string> GetProductNames
            (this AdventureWorks adventureWorks, decimal maxListPrice) =>
                GetProductNamesCompiled(adventureWorks, maxListPrice);
    }
}
