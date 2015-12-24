namespace Dixin.Linq.LinqToEntities
{
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Linq;

    public static class DbContextExtensions
    {
        public static EntityContainer GetContainer
            (this DbContext dbContext) => (dbContext as IObjectContextAdapter)
                .ObjectContext
                .MetadataWorkspace
                .GetItemCollection(DataSpace.CSpace)
                .GetItems<EntityContainer>()
                .Single();
    }
}
