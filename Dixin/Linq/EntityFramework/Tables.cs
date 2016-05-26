namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;

    public partial class AdventureWorks
    {
        public const string Production = nameof(Production); // Production schema.
    }

    [Table(nameof(ProductCategory), Schema = AdventureWorks.Production)]
    public partial class ProductCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductCategoryID { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        // Other columns are ignored.
    }

    public partial class AdventureWorks
    {
        public DbSet<ProductCategory> ProductCategories { get; set; }
    }

    [Table(nameof(ProductSubcategory), Schema = AdventureWorks.Production)]
    public partial class ProductSubcategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductSubcategoryID { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        public int ProductCategoryID { get; set; }
    }

    [Table(nameof(Product), Schema = AdventureWorks.Production)]
    public partial class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductID { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        public decimal ListPrice { get; set; }

        public int? ProductSubcategoryID { get; set; }

        // public string Style { get; set; }
    }

    [Table(nameof(ProductPhoto), Schema = AdventureWorks.Production)]
    public partial class ProductPhoto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductPhotoID { get; set; }

        [MaxLength(50)]
        public string LargePhotoFileName { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [ConcurrencyCheck]
        public DateTime ModifiedDate { get; set; }
    }

    [Table(nameof(ProductProductPhoto), Schema = AdventureWorks.Production)]
    public partial class ProductProductPhoto
    {
        [Key]
        [Column(Order = 0)]
        public int ProductID { get; set; }

        [Key]
        [Column(Order = 1)]
        public int ProductPhotoID { get; set; }
    }

    public partial class AdventureWorks
    {
        public DbSet<ProductSubcategory> ProductSubcategories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductPhoto> ProductPhotos { get; set; }
    }
}
