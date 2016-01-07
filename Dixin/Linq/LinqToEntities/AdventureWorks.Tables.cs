namespace Dixin.Linq.LinqToEntities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;

    [Table(nameof(ProductCategory), Schema = AdventureWorksDbContext.Production)]
    public partial class ProductCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductCategoryID { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; }
    }

    [Table(nameof(ProductSubcategory), Schema = AdventureWorksDbContext.Production)]
    public partial class ProductSubcategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductSubcategoryID { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; }
    }

    [Table(nameof(Product), Schema = AdventureWorksDbContext.Production)]
    public partial class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductID { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        public decimal ListPrice { get; set; }
    }

    [Table(nameof(ProductPhoto), Schema = AdventureWorksDbContext.Production)]
    public partial class ProductPhoto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductPhotoID { get; set; }

        [MaxLength(50)]
        public string LargePhotoFileName { get; set; }
    }

    public partial class AdventureWorksDbContext
    {
        public const string Production = nameof(Production);

        public DbSet<ProductCategory> ProductCategories { get; set; }

        public DbSet<ProductSubcategory> ProductSubcategories { get; set; }

        public DbSet<Product> Products { get; set; }
    }
}
