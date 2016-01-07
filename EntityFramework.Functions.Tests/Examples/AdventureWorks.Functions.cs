namespace EntityFramework.Functions.Tests.Examples
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Linq;

    public partial class AdventureWorksDbContext
    {
        public const string dbo = nameof(dbo);

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add functions to model.
            modelBuilder.AddFunctions<AdventureWorksDbContext>();
            modelBuilder.AddFunctions(typeof(AdventureWorksFunctions));
            modelBuilder.AddFunctions(typeof(BuiltInFunctions));
            modelBuilder.AddFunctions(typeof(NiladicFunctions));
        }

        // Defines stored procedure returning a single result type: 
        // - a ManagerEmployee sequence.
        [Function(FunctionType.StoredProcedure, nameof(uspGetManagerEmployees), Schema = dbo)]
        public ObjectResult<ManagerEmployee> uspGetManagerEmployees(int? BusinessEntityID)
        {
            ObjectParameter businessEntityIdParameter = BusinessEntityID.HasValue
                ? new ObjectParameter(nameof(BusinessEntityID), BusinessEntityID)
                : new ObjectParameter(nameof(BusinessEntityID), typeof(int));

            return this.ObjectContext().ExecuteFunction<ManagerEmployee>(
                nameof(this.uspGetManagerEmployees), businessEntityIdParameter);
        }

        private const string uspLogError = nameof(uspLogError);

        // Defines stored procedure accepting an output parameter.
        // Output parameter must be ObjectParameter, with ParameterAttribute.ClrType provided.
        [Function(FunctionType.StoredProcedure, uspLogError, Schema = dbo)]
        public int LogError([Parameter(DbType = "int", ClrType = typeof(int))]ObjectParameter ErrorLogID) =>
            this.ObjectContext().ExecuteFunction(uspLogError, ErrorLogID);

        // Defines stored procedure returning multiple result types: 
        // - a ProductCategory sequence.
        // - a ProductSubcategory sequence.
        [Function(FunctionType.StoredProcedure, nameof(uspGetCategoryAndSubCategory), Schema = dbo)]
        [ResultType(typeof(ProductCategory))]
        [ResultType(typeof(ProductSubcategory))]
        public ObjectResult<ProductCategory> uspGetCategoryAndSubCategory(int CategoryID)
        {
            ObjectParameter categoryIdParameter = new ObjectParameter(nameof(CategoryID), CategoryID);
            return this.ObjectContext().ExecuteFunction<ProductCategory>(
                nameof(this.uspGetCategoryAndSubCategory), categoryIdParameter);
        }

        // Defines table-valued function, which must return IQueryable<T>.
        [Function(FunctionType.TableValuedFunction, nameof(ufnGetContactInformation), Schema = dbo)]
        public IQueryable<ContactInformation> ufnGetContactInformation(
            [Parameter(DbType = "int", Name = "PersonID")]int? personId)
        {
            ObjectParameter personIdParameter = personId.HasValue
                ? new ObjectParameter("PersonID", personId)
                : new ObjectParameter("PersonID", typeof(int));

            return this.ObjectContext().CreateQuery<ContactInformation>(
                $"[{nameof(this.ufnGetContactInformation)}](@{nameof(personId)})", personIdParameter);
        }

        // Defines scalar-valued function (composable),
        // which can only be used in LINQ to Entities queries, where its body will never be executed;
        // and cannot be called directly.
        [Function(FunctionType.ComposableScalarValuedFunction, nameof(ufnGetProductListPrice), Schema = dbo)]
        [return: Parameter(DbType = "money")]
        public decimal? ufnGetProductListPrice(
            [Parameter(DbType = "int")] int ProductID,
            [Parameter(DbType = "datetime")] DateTime OrderDate) => 
                Function.CallNotSupported<decimal?>();

        // Defines scalar-valued function (non-composable), 
        // which cannot be used in LINQ to Entities queries;
        // and can be called directly.
        [Function(FunctionType.NonComposableScalarValuedFunction, nameof(ufnGetProductStandardCost), Schema = dbo)]
        [return: Parameter(DbType = "money")]
        public decimal? ufnGetProductStandardCost(
            [Parameter(DbType = "int")]int ProductID,
            [Parameter(DbType = "datetime")]DateTime OrderDate)
        {
            ObjectParameter productIdParameter = new ObjectParameter(nameof(ProductID), ProductID);
            ObjectParameter orderDateParameter = new ObjectParameter(nameof(OrderDate), OrderDate);
            return this.ObjectContext().ExecuteFunction<decimal?>(
                nameof(this.ufnGetProductStandardCost), productIdParameter, orderDateParameter).SingleOrDefault();
        }
    }
}
