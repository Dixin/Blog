namespace Dixin.Linq.LinqToEntities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ProductCategory
    {
        public virtual ICollection<ProductSubcategory> ProductSubcategories { get; } = new HashSet<ProductSubcategory>();
    }

    public partial class ProductSubcategory
    {
        public int? ProductCategoryID { get; set; }

        public virtual ProductCategory ProductCategory { get; set; }
    }

    public partial class ProductSubcategory
    {
        public virtual ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }

    public partial class Product
    {
        public virtual ProductSubcategory ProductSubcategory { get; set; }

        public int? ProductSubcategoryID { get; set; }
    }

    [Table(nameof(ProductProductPhoto), Schema = AdventureWorks.Production)]
    public partial class ProductProductPhoto
    {
    }

    public partial class Product
    {
        public virtual ICollection<ProductProductPhoto> ProductProductPhotos { get; } = new HashSet<ProductProductPhoto>();
    }

    public partial class ProductPhoto
    {
        public virtual ICollection<ProductProductPhoto> ProductProductPhotos { get; } = new HashSet<ProductProductPhoto>();
    }

    public partial class ProductProductPhoto
    {
        public virtual Product Product { get; set; }

        [Key]
        [Column(Order = 0)]
        [Index(IsUnique = true, IsClustered = false, Order = 1)]
        public int ProductID { get; set; }

        public virtual ProductPhoto ProductPhoto { get; set; }

        [Key]
        [Column(Order = 1)]
        [Index(IsUnique = true, IsClustered = false, Order = 2)]
        public int ProductPhotoID { get; set; }
    }
}
