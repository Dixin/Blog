namespace EntityFramework.Functions
{
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;

    public static class DbContextExtensions
    {
        public static ObjectContext ObjectContext
            (this DbContext context) => (context as IObjectContextAdapter)?.ObjectContext;
    }
}
