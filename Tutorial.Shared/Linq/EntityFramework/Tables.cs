namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
#if NETFX
    using System.Data.Entity;
#else

    using Microsoft.EntityFrameworkCore;
#endif

    public partial class WideWorldImporters
    {
        public const string Purchasing = nameof(Purchasing); // Purchasing schema.

        public const string Warehouse = nameof(Warehouse);

        public const string Sequences = nameof(Sequences);
    }

    [Table(nameof(WideWorldImporters.SupplierCategories), Schema = WideWorldImporters.Purchasing)]
    public partial class SupplierCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int SupplierCategoryID { get; set; }

        [MaxLength(50)]
        [Required]
        public string SupplierCategoryName { get; set; }

        public int LastEditedBy { get; set; } = 1;
    }

    public partial class WideWorldImporters
    {
        public DbSet<SupplierCategory> SupplierCategories { get; set; }
    }

    [Table(nameof(WideWorldImporters.Suppliers), Schema = WideWorldImporters.Purchasing)]
    public partial class Supplier
    {
        [Key]
        public int SupplierID { get; set; }

        [MaxLength(100)]
        [Required]
        public string SupplierName { get; set; }

        public int SupplierCategoryID { get; set; }

        [ConcurrencyCheck]
        public DateTime ValidFrom { get; set; }
    }

    [Table(nameof(WideWorldImporters.StockItems), Schema = WideWorldImporters.Warehouse)]
    public partial class StockItem
    {
        [Key]
        public int StockItemID { get; set; }

        [MaxLength(100)]
        [Required]
        public string StockItemName { get; set; }

        public decimal UnitPrice { get; set; }

        public int SupplierID { get; set; }
    }

    [Table(nameof(WideWorldImporters.StockItemHoldings), Schema = WideWorldImporters.Warehouse)]
    public partial class StockItemHolding
    {
        [Key]
        public int StockItemID { get; set; }

        [Required]
        [MaxLength(20)]
        public string BinLocation { get; set; }

        public int QuantityOnHand { get; set; }

        public decimal LastCostPrice { get; set; }
    }

    [Table(nameof(WideWorldImporters.StockGroups), Schema = WideWorldImporters.Warehouse)]
    public partial class StockGroup
    {
        [Key]
        public int StockGroupID { get; set; }

        [MaxLength(50)]
        public string StockGroupName { get; set; }

        public int LastEditedBy { get; set; } = 1;
    }

    public partial class WideWorldImporters
    {
        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<StockItem> StockItems { get; set; }

        public DbSet<StockGroup> StockGroups { get; set; }

        public DbSet<StockItemHolding> StockItemHoldings { get; set; }
    }
}
