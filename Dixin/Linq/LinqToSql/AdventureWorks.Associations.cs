
namespace Dixin.Linq.LinqToSql
{
    using System.Data.Linq;
    using System.Data.Linq.Mapping;

    public partial class ProductCategory
    {
        private readonly EntitySet<ProductSubcategory> productSubcategories = new EntitySet<ProductSubcategory>();

        [Association(ThisKey = nameof(ProductCategoryID), OtherKey = nameof(ProductSubcategory.ProductCategoryID))]
        public EntitySet<ProductSubcategory> ProductSubcategories
        {
            get { return this.productSubcategories; }
            set { this.productSubcategories.Assign(value); }
        }
    }

    public partial class ProductSubcategory
    {
        private int? productCategoryId;

        private EntityRef<ProductCategory> productCategory = new EntityRef<ProductCategory>();

        [Column(DbType = "int NOT NULL")]
        public int? ProductCategoryID
        {
            get { return this.productCategoryId; }
            set
            {
                this.productCategory.SetForeignKey(
                    () => this.productCategoryId == value, () => this.productCategoryId = value);
            }
        }

        [Association(IsForeignKey = true, ThisKey = nameof(ProductCategoryID), OtherKey = nameof(LinqToSql.ProductCategory.ProductCategoryID))]
        public ProductCategory ProductCategory
        {
            get { return this.productCategory.Entity; }
            set
            {
                this.Associate(
                    key => this.ProductCategoryID = key, 
                    this.productCategory,
                    value, 
                    () => value.ProductCategoryID, 
                    other => other.ProductSubcategories);
                // ProductCategory previousCategory = this.productCategory.Entity;
                // if (previousCategory != value || !this.productCategory.HasLoadedOrAssignedValue)
                // {
                //    if (previousCategory != null)
                //    {
                //        this.productCategory.Entity = null;
                //        previousCategory.ProductSubcategories.Remove(this);
                //    }
                //
                //    this.productCategory.Entity = value;
                //    if (value != null)
                //    {
                //        value.ProductSubcategories.Add(this);
                //        this.ProductCategoryID = value.ProductCategoryID;
                //    }
                //    else
                //    {
                //        this.ProductCategoryID = default(int);
                //    }
                // }
            }
        }
    }

    public partial class ProductSubcategory
    {
        private readonly EntitySet<Product> products = new EntitySet<Product>();

        [Association(ThisKey = nameof(ProductSubcategoryID), OtherKey = nameof(Product.ProductSubcategoryID))]
        public EntitySet<Product> Products
        {
            get { return this.products; }
            set { this.products.Assign(value); }
        }
    }

    public partial class Product
    {
        private int? productSubcategoryId;

        private EntityRef<ProductSubcategory> productSubcategory = new EntityRef<ProductSubcategory>();

        [Column(DbType = "int")]
        public int? ProductSubcategoryID
        {
            get { return this.productSubcategoryId; }
            set
            {
                this.productSubcategory.SetForeignKey(
                    () => this.productSubcategoryId == value, () => this.productSubcategoryId = value);
            }
        }

        [Association(IsForeignKey = true, ThisKey = nameof(ProductSubcategoryID), OtherKey = nameof(LinqToSql.ProductSubcategory.ProductSubcategoryID))]
        public ProductSubcategory ProductSubcategory
        {
            get { return this.productSubcategory.Entity; }
            set
            {
                this.Associate(
                    key => this.ProductSubcategoryID = key, 
                    this.productSubcategory,
                    value, 
                    () => value.ProductSubcategoryID, 
                    other => other.Products);
            }
        }
    }

    [Table(Name = "Production.ProductProductPhoto")]
    public partial class ProductProductPhoto
    {
    }

    public partial class Product
    {
        private readonly EntitySet<ProductProductPhoto> productProductPhotos = new EntitySet<ProductProductPhoto>();

        [Association(ThisKey = nameof(ProductID), OtherKey = nameof(ProductProductPhoto.ProductID))]
        public EntitySet<ProductProductPhoto> ProductProductPhotos
        {
            get { return this.productProductPhotos; }
            set { this.productProductPhotos.Assign(value); }
        }
    }

    public partial class ProductPhoto
    {
        private readonly EntitySet<ProductProductPhoto> productProductPhotos = new EntitySet<ProductProductPhoto>();

        [Association(ThisKey = nameof(ProductPhotoID), OtherKey = nameof(ProductProductPhoto.ProductPhotoID))]
        public EntitySet<ProductProductPhoto> ProductProductPhotos
        {
            get { return this.productProductPhotos; }
            set { this.productProductPhotos.Assign(value); }
        }
    }

    public partial class ProductProductPhoto
    {
        private int productId;

        private EntityRef<Product> product = new EntityRef<Product>();

        [Column(DbType = "int", IsPrimaryKey = true)]
        public int ProductID
        {
            get { return this.productId; }
            set
            {
                this.product.SetForeignKey(
                    () => this.ProductID == value, () => this.ProductID = value);
            }
        }

        [Association(IsForeignKey = true, ThisKey = nameof(ProductID), OtherKey = nameof(LinqToSql.Product.ProductID))]
        public Product Product
        {
            get { return this.product.Entity; }
            set
            {
                this.Associate(
                    key => this.ProductID = key, 
                    this.product,
                    value, 
                    () => value.ProductID, 
                    other => other.ProductProductPhotos);
            }
        }
    }

    public partial class ProductProductPhoto
    {
        private int productPhotoId;

        private EntityRef<ProductPhoto> productPhoto = new EntityRef<ProductPhoto>();

        [Column(DbType = "int", IsPrimaryKey = true)]
        public int ProductPhotoID
        {
            get { return this.productPhotoId; }
            set
            {
                this.product.SetForeignKey(
                    () => this.ProductPhotoID == value, () => this.ProductPhotoID = value);
            }
        }

        [Association(IsForeignKey = true, ThisKey = nameof(ProductPhotoID), OtherKey = nameof(LinqToSql.ProductPhoto.ProductPhotoID))]
        public ProductPhoto ProductPhoto
        {
            get { return this.productPhoto.Entity; }
            set
            {
                this.Associate(
                    key => this.ProductPhotoID = key, 
                    this.productPhoto,
                    value, 
                    () => value.ProductPhotoID, 
                    other => other.ProductProductPhotos);
            }
        }
    }

}
