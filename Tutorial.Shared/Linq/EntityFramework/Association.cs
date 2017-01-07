namespace Dixin.Linq.EntityFramework
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

#if NETFX
    using System.Data.Entity;
#else
    using Microsoft.EntityFrameworkCore;
#endif

    public partial class SupplierCategory
    {
        public virtual ICollection<Supplier> Suppliers { get; set; }
            = new HashSet<Supplier>();
    }

    public partial class Supplier
    {
        // public int? SupplierCategoryID { get; set; }
        public virtual SupplierCategory SupplierCategory { get; set; }
    }

    public partial class Supplier
    {
        public virtual ICollection<StockItem> StockItems { get; set; } = new HashSet<StockItem>();
    }

    public partial class StockItem
    {
        // public int? SupplierID { get; set; }
        public virtual Supplier Supplier { get; set; }
    }

    [Table(nameof(WideWorldImporters.StockItemStockGroups), Schema = WideWorldImporters.Warehouse)]
    public partial class StockItemStockGroup
    {
        [Key]
        public int StockItemStockGroupID { get; set; }

        public int StockItemID { get; set; }

        public int StockGroupID { get; set; }

        public int LastEditedBy { get; set; } = 1;
    }

    public partial class WideWorldImporters
    {
        public DbSet<StockItemStockGroup> StockItemStockGroups { get; set; }
    }

    public partial class StockItem
    {
        public virtual ICollection<StockItemStockGroup> StockItemStockGroups { get; set; }
            = new HashSet<StockItemStockGroup>();
    }

    public partial class StockGroup
    {
        public virtual ICollection<StockItemStockGroup> StockItemStockGroups { get; set; }
            = new HashSet<StockItemStockGroup>();
    }

    public partial class StockItemStockGroup
    {
        // public int StockItemID { get; set; }
        public virtual StockItem StockItem { get; set; }

        // public int StockGroupsID { get; set; }
        public virtual StockGroup StockGroup { get; set; }
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