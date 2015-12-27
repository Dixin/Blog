namespace Dixin.Linq.LinqToEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using System.IO;

    using Dixin.Common;
    using Dixin.Properties;

    public partial class AdventureWorks : DbContext
    {
        public const string ProductionSchema = "Production";

        static AdventureWorks()
        {
            // Initializes |DataDirectory| in connection string.
            // <connectionStrings>
            //  <add name="EntityFramework.Functions.Tests.Properties.Settings.AdventureWorksConnectionString"
            //    connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AdventureWorks_Data.mdf;Integrated Security=True;Connect Timeout=30"
            //    providerName="System.Data.SqlClient" />
            // </connectionStrings>
            AppDomainData.DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data");

            // Equivalent to: Database.SetInitializer(new NullDatabaseInitializer<AdventureWorks>());
            Database.SetInitializer<AdventureWorks>(null);
        }

        public AdventureWorks()
            : base(Settings.Default.AdventureWorksConnectionString)
        {
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
        public ObjectResult<ManagerEmployee> GetManagerEmployees(int BusinessEntityID)
        {
            SqlParameter businessEntityIdParameter = new SqlParameter(nameof(BusinessEntityID), BusinessEntityID);
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<ManagerEmployee>(
                $"[dbo].[uspGetManagerEmployees] @{nameof(BusinessEntityID)}",
                businessEntityIdParameter);
        }
    }
}
