namespace Dixin.Linq.EntityFramework
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ProductCategory
    {
        public virtual ICollection<ProductSubcategory> ProductSubcategories { get; set; }
            = new HashSet<ProductSubcategory>();
    }

    public partial class ProductSubcategory
    {
        // public int? ProductCategoryID { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }
    }

    public partial class ProductSubcategory
    {
        public virtual ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }

    public partial class Product
    {
        // public int? ProductSubcategoryID { get; set; }
        public virtual ProductSubcategory ProductSubcategory { get; set; }
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

    public partial class Product
    {
        public virtual ICollection<ProductProductPhoto> ProductProductPhotos { get; set; }
            = new HashSet<ProductProductPhoto>();
    }

    public partial class ProductPhoto
    {
        public virtual ICollection<ProductProductPhoto> ProductProductPhotos { get; set; }
            = new HashSet<ProductProductPhoto>();
    }

    public partial class ProductProductPhoto
    {
        // public int ProductID { get; set; }
        public virtual Product Product { get; set; }

        // public int ProductPhotoID { get; set; }
        public virtual ProductPhoto ProductPhoto { get; set; }
    }
}

#if DEMO
namespace Dixin.Linq.EntityFramework
{
    using System.Collections.Generic;
    using System.Data.Entity;

    public partial class Product
    {
        public virtual ICollection<ProductPhoto> ProductPhotos { get; set; }
            = new HashSet<ProductPhoto>();
    }

    public partial class ProductPhoto
    {
        public virtual ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }

    public partial class AdventureWorks
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Product>()
                .HasMany(product => product.ProductPhotos)
                .WithMany(photo => photo.Products)
                .Map(mapping => mapping
                    .ToTable("ProductProductPhoto", Production)
                    .MapLeftKey("ProductID")
                    .MapRightKey("ProductPhotoID"));
        }
    }
}
#endif