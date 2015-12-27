namespace EntityFramework.Functions
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Conventions;

    public class FunctionConvention : IStoreModelConvention<EntityContainer>
    {
        private readonly Type[] functionsTypes;

        public FunctionConvention(params Type[] functionsTypes)
        {
            this.functionsTypes = functionsTypes;
        }

        public void Apply(EntityContainer item, DbModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // item is ignored. It is just model.StoreModel.Container.
            this.functionsTypes.ForEach(model.AddFunctions);
        }
    }

    public class FunctionConvention<TFunctions> : IStoreModelConvention<EntityContainer>
    {
        private readonly FunctionConvention functionConvention;

        public FunctionConvention()
        {
            this.functionConvention = new FunctionConvention(typeof(TFunctions));
        }

        public void Apply(EntityContainer item, DbModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            this.functionConvention.Apply(item, model);
        }
    }
}
