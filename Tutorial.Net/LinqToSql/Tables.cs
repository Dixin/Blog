namespace Tutorial.LinqToSql
{
    using System;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;

    [Table(Name = "[Production].[ProductCategory]")]
    public partial class ProductCategory
    {
        [Column(DbType = "int NOT NULL IDENTITY", 
            IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.OnInsert)]
        public int ProductCategoryID { get; set; }

        [Column(DbType = "nvarchar(50) NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string Name { get; set; }

        // Other columns are ignored.
    }

    [Table(Name = "[Production].[ProductSubcategory]")]
    public partial class ProductSubcategory
    {
        [Column(DbType = "int NOT NULL IDENTITY", 
            IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.OnInsert)]
        public int ProductSubcategoryID { get; set; }

        [Column(DbType = "nvarchar(50) NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string Name { get; set; }

        // public int ProductCategoryID { get; set; }
    }

    [Table(Name = "[Production].[Product]")]
    public partial class Product
    {
        [Column(DbType = "int NOT NULL IDENTITY", 
            IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.OnInsert)]
        public int ProductID { get; set; }

        [Column(DbType = "nvarchar(50) NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string Name { get; set; }

        [Column(DbType = "money NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public decimal ListPrice { get; set; }

        // public int? ProductSubcategoryID { get; set; }

        // public string Style { get; set; }
    }

    [Table(Name = "[Production].[ProductPhoto]")]
    public partial class ProductPhoto
    {
        [Column(DbType = "int NOT NULL IDENTITY", UpdateCheck = UpdateCheck.Never, 
            IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.OnInsert)]
        public int ProductPhotoID { get; set; }

        [Column(DbType = "nvarchar(50)", UpdateCheck = UpdateCheck.Never)]
        public string LargePhotoFileName { get; set; }

        [Column(DbType = "datetime NOT NULL", UpdateCheck = UpdateCheck.Always)]
        public DateTime ModifiedDate { get; set; }
    }

    public partial class AdventureWorks
    {
        public Table<ProductCategory> ProductCategories => this.GetTable<ProductCategory>();

        public Table<ProductSubcategory> ProductSubcategories => this.GetTable<ProductSubcategory>();

        public Table<Product> Products => this.GetTable<Product>();

        public Table<ProductPhoto> ProductPhotos => this.GetTable<ProductPhoto>();
    }
}
