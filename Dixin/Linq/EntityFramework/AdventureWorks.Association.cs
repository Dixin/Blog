namespace Dixin.Linq.EntityFramework
{
    using System.Collections.Generic;

    public partial class ProductCategory
    {
        public virtual ICollection<ProductSubcategory> ProductSubcategories { get; } = new HashSet<ProductSubcategory>();
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
        // public int ProductID { get; set; }
        public virtual Product Product { get; set; }

        // public int ProductPhotoID { get; set; }
        public virtual ProductPhoto ProductPhoto { get; set; }        
    }
}
