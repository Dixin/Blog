namespace EntityFramework.Functions.Tests.Examples
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Linq;

    public partial class AdventureWorks
    {
        public const string DboSchema = "dbo";

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add functions to model.
            modelBuilder.AddFunctions<AdventureWorks>();
            modelBuilder.AddFunctions(typeof(AdventureWorksFunctions));
            modelBuilder.AddFunctions(typeof(BuiltInFunctions));
            modelBuilder.AddFunctions(typeof(NiladicFunctions));
        }

        // Defines stored procedure returning a single result: 
        // - a ManagerEmployee sequence.
        [Function(nameof(uspGetManagerEmployees), FunctionType.StoredProcedure, Schema = DboSchema)]
        public ObjectResult<ManagerEmployee> uspGetManagerEmployees(int? BusinessEntityID)
        {
            ObjectParameter businessEntityIdParameter = BusinessEntityID.HasValue
                ? new ObjectParameter(nameof(BusinessEntityID), BusinessEntityID)
                : new ObjectParameter(nameof(BusinessEntityID), typeof(int));

            return this.ObjectContext().ExecuteFunction<ManagerEmployee>(
                nameof(this.uspGetManagerEmployees), businessEntityIdParameter);
        }

        // Defines stored procedure accepting an output parameter.
        // Output parameter must be ObjectParameter, with ParameterAttribute.ClrType provided.
        [Function(nameof(uspLogError), FunctionType.StoredProcedure, Schema = DboSchema)]
        public int uspLogError(
            [Parameter(DbType = "int", ClrType = typeof(int))]ObjectParameter ErrorLogID) =>
                this.ObjectContext().ExecuteFunction(nameof(this.uspLogError), ErrorLogID);

        // Defines stored procedure returning a multiple results: 
        // - a ProductCategory sequence.
        // - a ProductSubcategory sequence.
        [Function(nameof(uspGetCategoryAndSubCategory), FunctionType.StoredProcedure, Schema = DboSchema)]
        [ResultType(typeof(ProductCategory))]
        [ResultType(typeof(ProductSubcategory))]
        public ObjectResult<ProductCategory> uspGetCategoryAndSubCategory(int CategoryID)
        {
            ObjectParameter categoryIdParameter = new ObjectParameter(nameof(CategoryID), CategoryID);
            return this.ObjectContext().ExecuteFunction<ProductCategory>(
                nameof(this.uspGetCategoryAndSubCategory), categoryIdParameter);
        }

        // Defines table-valued function, which must return IQueryable<T>.
        [Function(nameof(ufnGetContactInformation), FunctionType.TableValuedFunction, Schema = DboSchema)]
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
        [Function(nameof(ufnGetProductListPrice), FunctionType.ComposableScalarValuedFunction, Schema = DboSchema)]
        [return: Parameter(DbType = "money")]
        public decimal? ufnGetProductListPrice(
            [Parameter(DbType = "int")] int ProductID,
            [Parameter(DbType = "datetime")] DateTime OrderDate) => 
                Function.CallNotSupported<decimal?>(nameof(this.ufnGetProductListPrice));

        // Defines scalar-valued function (composable), 
        // which cannot be used in LINQ to Entities queries;
        // and can be called directly.
        [Function(nameof(ufnGetProductStandardCost), FunctionType.NonComposableScalarValuedFunction, Schema = DboSchema)]
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
