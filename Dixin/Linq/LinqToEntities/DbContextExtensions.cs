namespace Dixin.Linq.LinqToEntities
{
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Linq;

    public static class DbContextExtensions
    {
        public static EntityContainer Container
            (this DbContext context) => (context as IObjectContextAdapter)
                .ObjectContext
                .MetadataWorkspace
                .GetItemCollection(DataSpace.CSpace)
                .GetItems<EntityContainer>()
                .Single();

        public static ObjectContext ObjectContext
            (this DbContext context) => (context as IObjectContextAdapter).ObjectContext;

        public static TDbConnection Connection<TDbConnection>(this DbContext context)
            where TDbConnection:DbConnection => 
                (context.ObjectContext().Connection as EntityConnection)?.StoreConnection as TDbConnection;
    }
}
