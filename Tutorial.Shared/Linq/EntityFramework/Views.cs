namespace Dixin.Linq.EntityFramework
{
#if NETFX
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration;

    [Table(nameof(vProductAndDescription), Schema = WideWorldImporters.Purchasing)]
    public class vProductAndDescription
    {
        [Key]
        public int ProductID { get; set; }

        public string Name { get; set; }

        public string ProductModel { get; set; }

        public string CultureID { get; set; }

        public string Description { get; set; }
    }

    public class vProductAndDescriptionMapping : EntityTypeConfiguration<vProductAndDescription>
    {
        public vProductAndDescriptionMapping()
        {
            this.ToTable(nameof(vProductAndDescription));
        }
    }

    public partial class WideWorldImporters
    {
        public DbSet<vProductAndDescription> ProductAndDescriptions { get; set; }
    }
#endif
}
