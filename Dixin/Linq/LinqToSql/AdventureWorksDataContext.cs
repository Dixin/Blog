namespace Dixin.Linq.LinqToSql
{
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dixin.Common;
    using Dixin.Properties;
    using Dixin.Reflection;

    [Database(Name = "[AdventureWorks]")]
    public partial class AdventureWorksDataContext : DataContext
    {
        public AdventureWorksDataContext()
            : base(Settings.Default.AdventureWorksConnectionString)
        {
        }

        static AdventureWorksDataContext()
        {
            // Initializes |DataDirectory| in connection string.
            // <connectionStrings>
            //  <add name="EntityFramework.Functions.Tests.Properties.Settings.AdventureWorksConnectionString"
            //    connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AdventureWorks_Data.mdf;Integrated Security=True;Connect Timeout=30"
            //    providerName="System.Data.SqlClient" />
            // </connectionStrings>
            AppDomainData.DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data");
        }
    }

    [Table(Name = "[Production].[ProductCategory]")]
    public partial class ProductCategory
    {
        [Column(DbType = "Int NOT NULL IDENTITY", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.OnInsert)]
        public int ProductCategoryID { get; set; }

        [Column(DbType = "NVarChar(50) NOT NULL")]
        public string Name { get; set; }
    }

    [Table(Name = "[Production].[ProductSubcategory]")]
    public partial class ProductSubcategory
    {
        [Column(DbType = "Int NOT NULL IDENTITY", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.OnInsert)]
        public int ProductSubcategoryID { get; set; }

        [Column(DbType = "NVarChar(50) NOT NULL")]
        public string Name { get; set; }
    }

    [Table(Name = "[Production].[Product]")]
    public partial class Product
    {
        [Column(DbType = "Int NOT NULL IDENTITY", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.OnInsert)]
        public int ProductID { get; set; }

        [Column(DbType = "NVarChar(50) NOT NULL")]
        public string Name { get; set; }

        [Column(DbType = "Money NOT NULL")]
        public decimal ListPrice { get; set; }
    }

    public static class TableHelper
    {
        public static void SetToKey<TKey, TOther>(
            this TKey value, Func<bool> areEqual, Action<TKey> setKey, Func<EntityRef<TOther>> getEntityRef)
            where TOther : class
        {
            if (!areEqual())
            {
                if (getEntityRef().HasLoadedOrAssignedValue)
                {
                    throw new ForeignKeyReferenceAlreadyHasValueException();
                }

                setKey(value);
            }
        }

        public static void SetKey<TThis, TOther, TKey>(this TThis @this, TKey value, string key, string entity)
            where TOther : class
        {
            if (!EqualityComparer<TKey>.Default.Equals(@this.GetField<TKey>(key), value))
            {
                if (@this.GetField<EntityRef<TOther>>(entity).HasLoadedOrAssignedValue)
                {
                    throw new ForeignKeyReferenceAlreadyHasValueException();
                }

                @this.SetField(key, value);
            }
        }

        public static void Associate<TThis, TOther, TColumn>(
            this TThis @this,
            TOther other,
            Func<EntityRef<TOther>> getEntity,
            Func<TOther, EntitySet<TThis>> getEntitySet,
            Func<TColumn> getOtherKey,
            Action<TColumn> setThisKey)
            where TOther : class
            where TThis : class
        {
            EntityRef<TOther> entityRef = getEntity();
            TOther previousOther = entityRef.Entity;
            if (previousOther != other || !entityRef.HasLoadedOrAssignedValue)
            {
                if (previousOther != null)
                {
                    entityRef.Entity = null;
                    getEntitySet(previousOther).Remove(@this);
                }

                entityRef.Entity = other;
                if (other != null)
                {
                    getEntitySet(other).Add(@this);
                    setThisKey(getOtherKey());
                }
                else
                {
                    setThisKey(default(TColumn));
                }
            }
        }

        public static void Associate<TThis, TOther, TColumn>(
            this TThis @this, TOther other, string entity, string entitySet, string thisKey, string otherKey)
            where TOther : class
            where TThis : class
        {
            EntityRef<TOther> entityRef = @this.GetField<EntityRef<TOther>>(entity);
            TOther previousOther = entityRef.Entity;
            if (previousOther != other || !entityRef.HasLoadedOrAssignedValue)
            {
                if (previousOther != null)
                {
                    entityRef.Entity = null;
                    previousOther.GetProperty<EntitySet<TThis>>(entitySet).Remove(@this);
                }

                entityRef.Entity = other;
                if (other != null)
                {
                    other.GetProperty<EntitySet<TThis>>(entitySet).Add(@this);
                    @this.SetProperty(thisKey, other.GetProperty<TColumn>(otherKey));
                }
                else
                {
                    @this.SetProperty(thisKey, default(TColumn));
                }
            }
        }
    }

    public partial class ProductCategory
    {
        [Association(ThisKey = nameof(ProductCategoryID), OtherKey = nameof(ProductSubcategory.ProductCategoryID))]
        public EntitySet<ProductSubcategory> ProductSubcategories { get; } = new EntitySet<ProductSubcategory>();
    }

    public partial class ProductSubcategory
    {
        private int? productCategoryID;

        private EntityRef<ProductCategory> productCategory = new EntityRef<ProductCategory>();

        [Column(DbType = "Int NOT NULL")]
        public int? ProductCategoryID
        {
            get
            {
                return this.productCategoryID;
            }
            set
            {
                this.SetKey<ProductSubcategory, ProductCategory, int?>(
                    value, nameof(this.productCategoryID), nameof(this.productCategory));
                // value.SetToKey(
                //    () => this.productCategoryID == value,
                //    key => this.productCategoryID = key,
                //    () => this.productCategory);
                // if (this.productCategoryID != value)
                // {
                //    if (this.productCategory.HasLoadedOrAssignedValue)
                //    {
                //        throw new ForeignKeyReferenceAlreadyHasValueException();
                //    }
                //
                //    this.productCategoryID = value;
                // }
            }
        }

        [Association(Storage = nameof(productCategory), IsForeignKey = true, ThisKey = nameof(ProductCategoryID), OtherKey = nameof(LinqToSql.ProductCategory.ProductCategoryID))]
        public ProductCategory ProductCategory
        {
            get
            {
                return this.productCategory.Entity;
            }
            set
            {
                this.Associate<ProductSubcategory, ProductCategory, int>(
                    value,
                    nameof(this.productCategory),
                    nameof(value.ProductSubcategories),
                    nameof(this.ProductCategoryID),
                    nameof(value.ProductCategoryID));
                // this.Associate(value,
                //    () => this.productCategory,
                //    other => other.ProductSubcategories,
                //    () => value.ProductCategoryID,
                //    column => this.ProductCategoryID = column);
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
        [Association(ThisKey = nameof(ProductSubcategoryID), OtherKey = nameof(Product.ProductSubcategoryID))]
        public EntitySet<Product> Products { get; } = new EntitySet<Product>();
    }

    public partial class Product
    {
        private int? productSubcategoryID;

        private EntityRef<ProductSubcategory> productSubcategory = new EntityRef<ProductSubcategory>();
        
        [Association(Storage = nameof(productSubcategory), IsForeignKey = true, ThisKey = nameof(ProductSubcategoryID), OtherKey = nameof(LinqToSql.ProductSubcategory.ProductSubcategoryID))]
        public ProductSubcategory ProductSubcategory
        {
            get { return this.productSubcategory.Entity; }
            set
            {
                this.Associate(
                    value,
                    () => this.productSubcategory,
                    other => other.Products,
                    () => value.ProductSubcategoryID,
                    column => this.ProductSubcategoryID = column);
            }
        }

        [Column(DbType = "Int")]
        public int? ProductSubcategoryID
        {
            get
            {
                return this.productSubcategoryID;
            }
            set
            {
                this.SetKey<Product, ProductSubcategory, int?>(
                    value, nameof(this.productSubcategoryID), nameof(this.productSubcategory));
            }
        }
    }

    public partial class AdventureWorksDataContext
    {
        public Table<ProductCategory> ProductCategories => this.GetTable<ProductCategory>();

        public Table<ProductSubcategory> ProductSubcategories => this.GetTable<ProductSubcategory>();

        public Table<Product> Products => this.GetTable<Product>();
    }

    public partial class AdventureWorksDataContext
    {
        //[Function(Name = "[dbo].[uspGetManagerEmployees]")]
        //public ISingleResult<ManagerEmployee> uspGetManagerEmployees(
        //    [Parameter(DbType = "Int")] int? BusinessEntityID)
        //{
        //    IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), BusinessEntityID);
        //    return (ISingleResult<ManagerEmployee>)result.ReturnValue;
        //}

        //[Function(Name = "[dbo].[ufnGetContactInformation]", IsComposable = true)]
        //public IQueryable<ContactInformation> ufnGetContactInformation(
        //    [Parameter(DbType = "Int")] int? PersonID) => 
        //        this.CreateMethodCallQuery<ContactInformation>(
        //            this, (MethodInfo)MethodBase.GetCurrentMethod(), PersonID);

        [Function(Name = "dbo.uspGetManagerEmployees")]
        public ISingleResult<ManagerEmployee> uspGetManagerEmployees(
            [Parameter(Name = "BusinessEntityID", DbType = "Int")] int? businessEntityID)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), businessEntityID);
            return (ISingleResult<ManagerEmployee>)result.ReturnValue;
        }

        [Function(Name = "dbo.uspLogError")]
        public int uspLogError(
            [Parameter(Name = "ErrorLogID", DbType = "Int")] ref int? errorLogID)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), errorLogID);
            errorLogID = (int?)result.GetParameterValue(0);
            return (int)result.ReturnValue;
        }

        [Function(Name = "dbo.uspGetCategoryAndSubCategory")]
        [ResultType(typeof(ProductCategory))]
        [ResultType(typeof(ProductSubcategory))]
        public IMultipleResults uspGetCategoryAndSubCategory(
            [Parameter(Name = "CategoryID", DbType = "Int")] int? categoryID)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), categoryID);
            return (IMultipleResults)result.ReturnValue;
        }

        [Function(Name = "dbo.ufnGetContactInformation", IsComposable = true)]
        public IQueryable<ContactInformation> ufnGetContactInformation(
            [Parameter(Name = "PersonID", DbType = "Int")] int? personID) => 
                this.CreateMethodCallQuery<ContactInformation>(
                    this, (MethodInfo)MethodBase.GetCurrentMethod(), personID);

        [Function(Name = "dbo.ufnGetProductListPrice", IsComposable = true)]
        public decimal? ufnGetProductListPrice(
            [Parameter(Name = "ProductID", DbType = "Int")] int? productID, 
            [Parameter(Name = "OrderDate", DbType = "DateTime")] DateTime? orderDate) => 
                (decimal?)this.ExecuteMethodCall(
                    this, (MethodInfo)MethodBase.GetCurrentMethod(), productID, orderDate).ReturnValue;

        [Function(Name = "dbo.ufnGetProductStandardCost", IsComposable = true)]
        public decimal? ufnGetProductStandardCost(
            [Parameter(Name = "ProductID", DbType = "Int")] int? productID, 
            [Parameter(Name = "OrderDate", DbType = "DateTime")] DateTime? orderDate) => 
                (decimal?)this.ExecuteMethodCall(
                    this, (MethodInfo)MethodBase.GetCurrentMethod(), productID, orderDate).ReturnValue;
    }

    public partial class ContactInformation
    {
        [Column(DbType = "Int NOT NULL")]
        public int PersonID { get; set; }

        [Column(DbType = "NVarChar(50)")]
        public string FirstName { get; set; }

        [Column(DbType = "NVarChar(50)")]
        public string LastName { get; set; }

        [Column(DbType = "NVarChar(50)")]
        public string JobTitle { get; set; }

        [Column(DbType = "NVarChar(50)")]
        public string BusinessEntityType { get; set; }
    }

    public partial class ManagerEmployee
    {
        [Column(DbType = "Int")]
        public int? RecursionLevel { get; set; }

        [Column(DbType = "NVarChar(4000)")]
        public string OrganizationNode { get; set; }

        [Column(DbType = "NVarChar(50) NOT NULL", CanBeNull = false)]
        public string ManagerFirstName { get; set; }

        [Column(DbType = "NVarChar(50) NOT NULL", CanBeNull = false)]
        public string ManagerLastName { get; set; }

        [Column(DbType = "Int")]
        public int? BusinessEntityID { get; set; }

        [Column(DbType = "NVarChar(50)")]
        public string FirstName { get; set; }

        [Column(DbType = "NVarChar(50)")]
        public string LastName { get; set; }
    }
}
