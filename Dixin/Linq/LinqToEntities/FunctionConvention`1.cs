namespace Dixin.Linq.LinqToEntities
{
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Linq;
    using System.Reflection;

    public class FunctionConvention<TDbContext> : IStoreModelConvention<EntityContainer>
    {
        private readonly DbModelBuilder modelBuilder;

        public FunctionConvention(DbModelBuilder modelBuilder)
        {
            this.modelBuilder = modelBuilder;
        }

        public void Apply(EntityContainer item, DbModel model) => typeof(TDbContext) 
            .GetMethods(BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance)
            .Select(methodInfo => new
                {
                    MethodInfo = methodInfo,
                    FunctionAttribute = methodInfo.GetCustomAttribute<FunctionAttribute>()
                })
            .Where(method => method.FunctionAttribute != null)
            .ForEach(method =>
                // item is ignored. It is just model.StoreModel.Container.
                model.AddFunction(method.MethodInfo, method.FunctionAttribute, this.modelBuilder));
    }
}
