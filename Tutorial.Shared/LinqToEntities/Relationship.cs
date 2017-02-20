namespace Tutorial.LinqToEntities
{
#if EF
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    
    using ModelBuilder = System.Data.Entity.DbModelBuilder;
#else
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Microsoft.EntityFrameworkCore;
#endif

    public partial class AdventureWorks
    {
        public const string Person = nameof(Person);

        public const string HumanResources = nameof(HumanResources);
    }

    [Table(nameof(Person), Schema = AdventureWorks.Person)]
    public partial class Person
    {
        [Key]
        public int BusinessEntityID { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        public virtual Employee Employee { get; set; } // Reference navigation property.
    }

    [Table(nameof(Employee), Schema = AdventureWorks.HumanResources)]
    public partial class Employee
    {
        [Key]
        [ForeignKey(nameof(Person))]
        public int BusinessEntityID { get; set; }

        [Required]
        [MaxLength(50)]
        public string JobTitle { get; set; }

        public DateTime HireDate { get; set; }

        public virtual Person Person { get; set; } // Reference navigation property.
    }

    public partial class ProductCategory
    {
        public virtual ICollection<ProductSubcategory> ProductSubcategories { get; set; } // Collection navigation property.
    }

    [Table(nameof(ProductSubcategory), Schema = AdventureWorks.Production)]
    public partial class ProductSubcategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductSubcategoryID { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        public int ProductCategoryID { get; set; }

        public virtual ProductCategory ProductCategory { get; set; } // Reference navigation property.

        public virtual ICollection<Product> Products { get; set; } // Collection navigation property.
    }

    [Table(nameof(Product), Schema = AdventureWorks.Production)]
    public partial class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductID { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        public decimal ListPrice { get; set; }

        public int? ProductSubcategoryID { get; set; }

        public virtual ProductSubcategory ProductSubcategory { get; set; } // Reference navigation property.
    }

    public partial class Product
    {
        public virtual ICollection<ProductProductPhoto> ProductProductPhotos { get; set; } // Collection navigation property.
    }

    [Table(nameof(ProductPhoto), Schema = AdventureWorks.Production)]
    public partial class ProductPhoto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductPhotoID { get; set; }

        [MaxLength(50)]
        public string LargePhotoFileName { get; set; }

        [ConcurrencyCheck]
        public DateTime ModifiedDate { get; set; }

        public virtual ICollection<ProductProductPhoto> ProductProductPhotos { get; set; } // Collection navigation property.
    }

    [Table(nameof(ProductProductPhoto), Schema = AdventureWorks.Production)]
    public partial class ProductProductPhoto
    {
        [Key]
        [Column(Order = 0)]
        public int ProductID { get; set; }

        [Key]
        [Column(Order = 1)]
        public int ProductPhotoID { get; set; }

        public virtual Product Product { get; set; } // Reference navigation property.

        public virtual ProductPhoto ProductPhoto { get; set; } // Reference navigation property.
    }

    public partial class AdventureWorks
    {
        private void MapCompositePrimaryKey(ModelBuilder modelBuilder) // Called by OnModelCreating.
        {
#if !EF
            modelBuilder.Entity<ProductProductPhoto>()
                .HasKey(productProductPhoto => new
                {
                    ProductID = productProductPhoto.ProductID,
                    ProductPhotoID = productProductPhoto.ProductPhotoID
                });
#endif
        }
    }

    public partial class AdventureWorks
    {
        private void MapManyToMany(ModelBuilder modelBuilder) // Called by OnModelCreating.
        {
#if !EF
            modelBuilder.Entity<ProductProductPhoto>()
                .HasOne(productProductPhoto => productProductPhoto.Product)
                .WithMany(product => product.ProductProductPhotos)
                .HasForeignKey(productProductPhoto => productProductPhoto.ProductID);

            modelBuilder.Entity<ProductProductPhoto>()
                .HasOne(productProductPhoto => productProductPhoto.ProductPhoto)
                .WithMany(photo => photo.ProductProductPhotos)
                .HasForeignKey(productProductPhoto => productProductPhoto.ProductPhotoID);
#endif
        }
    }

    public partial class AdventureWorks
    {
        public DbSet<Person> People { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<ProductSubcategory> ProductSubcategories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductPhoto> ProductPhotos { get; set; }
    }
}

#if DEMO
namespace Tutorial.LinqToEntities
{
    using System.Collections.Generic;
    using System.Data.Entity;

#if EF
    public partial class Product
    {
        public virtual ICollection<ProductPhoto> ProductPhotos { get; set; }
    }

    public partial class ProductPhoto
    {
        public virtual ICollection<Product> Products { get; set; }
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
                    .MapLeftKey(nameof(Product.ProductID))
                    .MapRightKey(nameof(ProductPhoto.ProductPhotoID)));
        }
    }
#endif
}
#endif