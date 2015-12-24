using System;

namespace Dixin.Linq.LinqToEntities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.SqlClient;
    using System.Linq;

    using Dixin.Properties;

    public partial class AdventureWorks : DbContext
    {
        public const string ProductionSchema = "Production";

        public AdventureWorks()
            : base(Settings.Default.AdventureWorksConnectionString)
        {
        }
    }

    public partial class AdventureWorks
    {
        static AdventureWorks()
        {
            Database.SetInitializer<AdventureWorks>(null);
            // Equivalent to: Database.SetInitializer(new NullDatabaseInitializer<AdventureWorks>());
        }
    }

    [Table(nameof(ProductCategory), Schema = AdventureWorks.ProductionSchema)]
    public partial class ProductCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductCategoryID { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }
    }

    [Table(nameof(ProductSubcategory), Schema = AdventureWorks.ProductionSchema)]
    public partial class ProductSubcategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductSubcategoryID { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }
    }

    [Table(nameof(Product), Schema = AdventureWorks.ProductionSchema)]
    public partial class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductID { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        public decimal ListPrice { get; set; }
    }

    public partial class AdventureWorks
    {
        public DbSet<ProductCategory> ProductCategories { get; set; }

        public DbSet<ProductSubcategory> ProductSubcategories { get; set; }

        public DbSet<Product> Products { get; set; }
    }

    public partial class ProductCategory
    {
        public virtual ICollection<ProductSubcategory> ProductSubcategories { get; } = new HashSet<ProductSubcategory>();
    }

    public partial class ProductSubcategory
    {
        public int? ProductCategoryID { get; set; }

        public ProductCategory ProductCategory { get; set; }
    }

    public partial class ProductSubcategory
    {
        public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }

    public partial class Product
    {
        public ProductSubcategory ProductSubcategory { get; set; }

        public int? ProductSubcategoryID { get; set; }
    }

    [ComplexType]
    public class ContactInformation
    {
        public int PersonID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string JobTitle { get; set; }

        public string BusinessEntityType { get; set; }
    }

    [ComplexType]
    public class ManagerEmployee
    {
        public int? RecursionLevel { get; set; }

        public string OrganizationNode { get; set; }

        public string ManagerFirstName { get; set; }

        public string ManagerLastName { get; set; }

        public int? BusinessEntityID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
    public partial class AdventureWorks
    {
        public const string DboSchema = "dbo";

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.FunctionConvention<AdventureWorks>();
        }

        public ObjectResult<ManagerEmployee> GetManagerEmployees(int BusinessEntityID)
        {
            SqlParameter businessEntityIdParameter = new SqlParameter(nameof(BusinessEntityID), BusinessEntityID);
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<ManagerEmployee>(
                $"[{DboSchema}].[{nameof(this.uspGetManagerEmployees)}] @{nameof(BusinessEntityID)}", 
                businessEntityIdParameter);
        }

        [Function(name: nameof(uspGetManagerEmployees), Schema = DboSchema)]
        public ObjectResult<ManagerEmployee> uspGetManagerEmployees(int? BusinessEntityID)
        {
            ObjectParameter businessEntityIdParameter = BusinessEntityID.HasValue
                ? new ObjectParameter(nameof(BusinessEntityID), BusinessEntityID)
                : new ObjectParameter(nameof(BusinessEntityID), typeof(int));

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<ManagerEmployee>(
                nameof(this.uspGetManagerEmployees), businessEntityIdParameter);
        }

        [Function(name: nameof(uspLogError), Schema = DboSchema)]
        public int uspLogError(
            [Parameter(DbType = "int", ClrType = typeof(int))]ObjectParameter ErrorLogID) => 
                ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction(
                    nameof(this.uspLogError), ErrorLogID);

        [Function(name: nameof(uspGetCategoryAndSubCategory), Schema = DboSchema)]
        [ResultType(typeof(ProductCategory))]
        [ResultType(typeof(ProductSubcategory))]
        public ObjectResult<ProductCategory> uspGetCategoryAndSubCategory(int CategoryID)
        {
            ObjectParameter categoryIdParameter = new ObjectParameter(nameof(CategoryID), CategoryID);
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<ProductCategory>(
                nameof(this.uspGetCategoryAndSubCategory), categoryIdParameter);
        }

        [Function(name: nameof(ufnGetContactInformation), Schema = DboSchema)]
        public IQueryable<ContactInformation> ufnGetContactInformation(
            [Parameter(DbType = "int", Name = "PersonID")]int? personId)
        {
            ObjectParameter personIdParameter = personId.HasValue
                ? new ObjectParameter("PersonID", personId)
                : new ObjectParameter("PersonID", typeof(int));

            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<ContactInformation>(
                $"[{nameof(this.ufnGetContactInformation)}](@{nameof(personId)})", personIdParameter);
        }

        [Function(name: nameof(ufnGetProductListPrice), isComposable: true, Schema = DboSchema)]
        [return: Parameter(DbType = "money")]
        public decimal? ufnGetProductListPrice(
            [Parameter(DbType = "int")]int ProductID,
            [Parameter(DbType = "datetime")]DateTime OrderDate)
        {
            throw new NotSupportedException();
        }

        [Function(name: nameof(ufnGetProductStandardCost), Schema = DboSchema)]
        [return: Parameter(DbType = "money")]
        public decimal? ufnGetProductStandardCost(
            [Parameter(DbType = "int")]int ProductID,
            [Parameter(DbType = "datetime")]DateTime OrderDate)
        {
            ObjectParameter productIdParameter = new ObjectParameter(nameof(ProductID), ProductID);
            ObjectParameter orderDateParameter = new ObjectParameter(nameof(OrderDate), OrderDate);
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<decimal?>(
                nameof(this.ufnGetProductStandardCost), productIdParameter, orderDateParameter).SingleOrDefault();
        }
    }
}
