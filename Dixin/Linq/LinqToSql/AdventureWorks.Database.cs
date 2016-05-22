namespace Dixin.Linq.LinqToSql
{
    using System.Data.Linq;
    using System.Data.Linq.Mapping;

    [Database(Name = "[AdventureWorks]")]
    public partial class AdventureWorks : DataContext
    {
        public AdventureWorks()
            : base(Linq.ConnectionStrings.AdventureWorks)
        {
            // if (!this.DatabaseExists())
            // {
            //    this.CreateDatabase();
            // }
        }
    }

    public partial class AdventureWorks
    {
        public Table<ProductCategory> ProductCategories => this.GetTable<ProductCategory>();

        public Table<ProductSubcategory> ProductSubcategories => this.GetTable<ProductSubcategory>();

        public Table<Product> Products => this.GetTable<Product>();

        public Table<ProductPhoto> ProductPhotos => this.GetTable<ProductPhoto>();
    }

    public partial class AdventureWorks
    {
        public Table<vProductAndDescription> ProductAndDescriptions => this.GetTable<vProductAndDescription>();
    }
}
