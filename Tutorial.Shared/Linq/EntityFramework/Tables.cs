namespace Dixin.Linq.EntityFramework
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
#if EF
    using System.Data.Entity;
#else

    using Microsoft.EntityFrameworkCore;
#endif

    public partial class AdventureWorks
    {
        public const string Production = nameof(Production);
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
}
