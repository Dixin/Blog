namespace EntityFramework.Functions.Tests.Examples
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;

    public partial class AdventureWorks
    {
        public const string Production = nameof(Production);

        public DbSet<ProductCategory> ProductCategories { get; set; }

        public DbSet<ProductSubcategory> ProductSubcategories { get; set; }

        public DbSet<Product> Products { get; set; }
    }


    [Table(nameof(ProductCategory), Schema = AdventureWorks.Production)]
    public partial class ProductCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductCategoryID { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }
    }

    [Table(nameof(ProductSubcategory), Schema = AdventureWorks.Production)]
    public partial class ProductSubcategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductSubcategoryID { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }
    }

    [Table(nameof(Product), Schema = AdventureWorks.Production)]
    public partial class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductID { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        public decimal ListPrice { get; set; }
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
}
