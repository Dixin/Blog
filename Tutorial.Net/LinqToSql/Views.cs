namespace Tutorial.LinqToSql
{
    using System.Data.Linq;
    using System.Data.Linq.Mapping;

    [Table(Name = "[Production].[vProductAndDescription]")]
    public class vProductAndDescription
    {
        [Column(DbType = "int NOT NULL")]
        public int ProductID { get; set; }

        [Column(DbType = "nvarchar(50) NOT NULL")]
        public string Name { get; set; }

        [Column(DbType = "nvarchar(50) NOT NULL")]
        public string ProductModel { get; set; }

        [Column(DbType = "nchar(6) NOT NULL")]
        public string CultureID { get; set; }

        [Column(DbType = "nvarchar(400) NOT NULL")]
        public string Description { get; set; }
    }

    public partial class AdventureWorks
    {
        public Table<vProductAndDescription> ProductAndDescriptions => this.GetTable<vProductAndDescription>();
    }
}
