namespace Tutorial.LinqToSql
{
    using System.Data.Linq;
    using System.Data.Linq.Mapping;

    public partial class ProductCategory
    {
        private readonly EntitySet<ProductSubcategory> productSubcategories;

        public ProductCategory()
        {
            this.productSubcategories = new EntitySet<ProductSubcategory>(
                subcategory => subcategory.ProductCategory = this, subcategory => subcategory.ProductCategory = null);
        }

        [Association(Storage = nameof(productSubcategories),
            ThisKey = nameof(ProductCategoryID), OtherKey = nameof(ProductSubcategory.ProductCategoryID))]
        public EntitySet<ProductSubcategory> ProductSubcategories
        {
            get { return this.productSubcategories; }
            set { this.productSubcategories.Assign(value); }
        }
    }

    public partial class ProductSubcategory
    {
        private int? productCategoryId;

        private EntityRef<ProductCategory> productCategory = default;

        [Column(DbType = "int NOT NULL", Storage = nameof(productCategoryId), UpdateCheck = UpdateCheck.Never)]
        public int? ProductCategoryID // Foreign key must be null.
        {
            get { return this.productCategoryId; }
            set
            {
                this.productCategory.SetForeignKey(
                    () => this.productCategoryId == value, () => this.productCategoryId = value);
            }
        }

        [Association(Storage = nameof(productCategory),
            IsForeignKey = true, ThisKey = nameof(ProductCategoryID), OtherKey = nameof(LinqToSql.ProductCategory.ProductCategoryID))]
        public ProductCategory ProductCategory
        {
            get { return this.productCategory.Entity; }
            set
            {
                this.Associate(
                    () => this.productCategoryId = value?.ProductCategoryID,
                    ref this.productCategory,
                    value,
                    other => other.ProductSubcategories);
                // ProductCategory previousCategory = this.productCategory.Entity;
                // if (previousCategory != value || !this.productCategory.HasLoadedOrAssignedValue)
                // {
                //    if (previousCategory != null)
                //    {
                //        this.productCategory.Entity = null;
                //        previousCategory.ProductSubcategories.Remove(this);
                //    }

                //    this.productCategory.Entity = value;
                //    if (value != null)
                //    {
                //        value.ProductSubcategories.Add(this);
                //        this.productCategoryId = value.ProductCategoryID;
                //    }
                //    else
                //    {
                //        this.productCategoryId = default;
                //    }
                // }
            }
        }
    }

    public partial class ProductSubcategory
    {
        private readonly EntitySet<Product> products;

        public ProductSubcategory()
        {
            this.products = new EntitySet<Product>(
                product => product.ProductSubcategory = this, product => product.ProductSubcategory = null);
        }

        [Association(Storage = nameof(products), 
            ThisKey = nameof(ProductSubcategoryID), OtherKey = nameof(Product.ProductSubcategoryID))]
        public EntitySet<Product> Products
        {
            get { return this.products; }
            set { this.products.Assign(value); }
        }
    }

    public partial class Product
    {
        private int? productSubcategoryId;

        private EntityRef<ProductSubcategory> productSubcategory = default;

        [Column(DbType = "int", Storage = nameof(productSubcategoryId))]
        public int? ProductSubcategoryID
        {
            get { return this.productSubcategoryId; }
            set
            {
                this.productSubcategory.SetForeignKey(
                    () => this.productSubcategoryId == value, () => this.productSubcategoryId = value);
            }
        }

        [Association(Storage = nameof(productSubcategory),
            IsForeignKey = true, ThisKey = nameof(ProductSubcategoryID), OtherKey = nameof(LinqToSql.ProductSubcategory.ProductSubcategoryID))]
        public ProductSubcategory ProductSubcategory
        {
            get { return this.productSubcategory.Entity; }
            set
            {
                this.Associate(
                    () => this.productSubcategoryId = value?.ProductSubcategoryID,
                    ref this.productSubcategory,
                    value,
                    other => other.Products);
            }
        }
    }

    [Table(Name = "Production.ProductProductPhoto")]
    public partial class ProductProductPhoto { }

    public partial class Product
    {
        private readonly EntitySet<ProductProductPhoto> productProductPhotos;

        public Product()
        {
            this.productProductPhotos = new EntitySet<ProductProductPhoto>(
                productProductPhoto => productProductPhoto.Product = this, productProductPhoto => productProductPhoto.Product = null);
        }

        [Association(Storage = nameof(productProductPhotos),
            ThisKey = nameof(ProductID), OtherKey = nameof(ProductProductPhoto.ProductID))]
        public EntitySet<ProductProductPhoto> ProductProductPhotos
        {
            get { return this.productProductPhotos; }
            set { this.productProductPhotos.Assign(value); }
        }
    }

    public partial class ProductPhoto
    {
        private readonly EntitySet<ProductProductPhoto> productProductPhotos;

        public ProductPhoto()
        {
            this.productProductPhotos = new EntitySet<ProductProductPhoto>(
                productProductPhoto => productProductPhoto.ProductPhoto = this, 
                productProductPhoto => productProductPhoto.ProductPhoto = null);
        }

        [Association(Storage = nameof(productProductPhotos),
            ThisKey = nameof(ProductPhotoID), OtherKey = nameof(ProductProductPhoto.ProductPhotoID))]
        public EntitySet<ProductProductPhoto> ProductProductPhotos
        {
            get { return this.productProductPhotos; }
            set { this.productProductPhotos.Assign(value); }
        }
    }

    public partial class ProductProductPhoto
    {
        private int? productId;

        private EntityRef<Product> product = default;

        [Column(DbType = "int", Storage = nameof(productId), IsPrimaryKey = true)]
        public int? ProductID
        {
            get { return this.productId; }
            set
            {
                this.product.SetForeignKey(
                    () => this.productId == value, () => this.productId = value);
            }
        }

        [Association(Storage = nameof(product),
            IsForeignKey = true, ThisKey = nameof(ProductID), OtherKey = nameof(LinqToSql.Product.ProductID))]
        public Product Product
        {
            get { return this.product.Entity; }
            set
            {
                this.Associate(
                    () => this.productId = value?.ProductID,
                    ref this.product,
                    value,
                    other => other.ProductProductPhotos);
            }
        }
    }

    public partial class ProductProductPhoto
    {
        private int? productPhotoId;

        private EntityRef<ProductPhoto> productPhoto = default;

        [Column(DbType = "int", Storage = nameof(productPhotoId), IsPrimaryKey = true)]
        public int? ProductPhotoID
        {
            get { return this.productPhotoId; }
            set
            {
                this.product.SetForeignKey(
                    () => this.productPhotoId == value, () => this.productPhotoId = value);
            }
        }

        [Association(Storage = nameof(productPhoto),
            IsForeignKey = true, ThisKey = nameof(ProductPhotoID), OtherKey = nameof(LinqToSql.ProductPhoto.ProductPhotoID))]
        public ProductPhoto ProductPhoto
        {
            get { return this.productPhoto.Entity; }
            set
            {
                this.Associate(
                    () => this.productPhotoId = value?.ProductPhotoID,
                    ref this.productPhoto,
                    value,
                    other => other.ProductProductPhotos);
            }
        }
    }

}
