namespace Tutorial.LinqToEntities
{
#if EF
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.MappingViews;
    using System.Linq;

    public static partial class DbContextExtensions
    {
        public static EntityContainer Container
            (this DbContext context) => ((IObjectContextAdapter)context)
                .ObjectContext
                .MetadataWorkspace
                .GetItemCollection(DataSpace.CSpace)
                .GetItems<EntityContainer>()
                .Single();

        public static ObjectContext ObjectContext
            (this IObjectContextAdapter context) => context.ObjectContext;

        public static TDbConnection Connection<TDbConnection>(this DbContext context)
            where TDbConnection : DbConnection =>
                ((EntityConnection)context.ObjectContext().Connection)?.StoreConnection as TDbConnection;
    }

    public static partial class DbContextExtensions
    {
        public static IDictionary<EntitySetBase, DbMappingView> GeteMappingViews(
            this IObjectContextAdapter context, out IList<EdmSchemaError> errors)
        {
            StorageMappingItemCollection mappings = (StorageMappingItemCollection)context.ObjectContext
                .MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
            errors = new List<EdmSchemaError>();
            return mappings.GenerateViews(errors);
        }
    }
#endif
}
