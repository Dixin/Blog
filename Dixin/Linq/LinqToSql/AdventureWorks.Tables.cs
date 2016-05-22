namespace Dixin.Linq.LinqToSql
{
    using System.Data.Linq.Mapping;

    [Table(Name = "[Production].[ProductCategory]")]
    public partial class ProductCategory
    {
        [Column(DbType = "int NOT NULL IDENTITY", 
            IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.OnInsert)]
        public int ProductCategoryID { get; set; }

        [Column(DbType = "nvarchar(50) NOT NULL")]
        public string Name { get; set; }
    }

    [Table(Name = "[Production].[ProductSubcategory]")]
    public partial class ProductSubcategory
    {
        [Column(DbType = "int NOT NULL IDENTITY", 
            IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.OnInsert)]
        public int ProductSubcategoryID { get; set; }

        [Column(DbType = "nvarchar(50) NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string Name { get; set; }
    }

    [Table(Name = "[Production].[Product]")]
    public partial class Product
    {
        [Column(DbType = "int NOT NULL IDENTITY", 
            IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.OnInsert)]
        public int ProductID { get; set; }

        [Column(DbType = "nvarchar(50) NOT NULL")]
        public string Name { get; set; }

        [Column(DbType = "money NOT NULL")]
        public decimal ListPrice { get; set; }
    }

    [Table(Name = "[Production].[ProductPhoto]")]
    public partial class ProductPhoto
    {
        [Column(DbType = "int NOT NULL IDENTITY", UpdateCheck = UpdateCheck.Never, 
            IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.OnInsert)]
        public int ProductPhotoID { get; set; }

        [Column(DbType = "nvarchar(50)", UpdateCheck = UpdateCheck.Never)]
        public string LargePhotoFileName { get; set; }
    }
}
