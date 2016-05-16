using System.Data.Entity.Infrastructure.MappingViews;

using Dixin.Linq.EntityFramework;

[assembly: DbMappingViewCacheType(typeof(AdventureWorks), typeof(AdventureWorksMappingViewCache))]

namespace Dixin.Linq.EntityFramework
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;

    internal class AdventureWorksMappingViewCache : DbMappingViewCache
    {
        private const string CodeFirstDatabase = nameof(CodeFirstDatabase);

        private static readonly Dictionary<string, DbMappingView> Views = new Dictionary<string, DbMappingView>()
        {
            [$"{CodeFirstDatabase}.{nameof(ProductCategory)}"] = new DbMappingView(@"
                SELECT VALUE -- Constructing ProductCategory
                    [CodeFirstDatabaseSchema.ProductCategory](T1.ProductCategory_ProductCategoryID, T1.ProductCategory_Name)
                FROM (
                    SELECT 
                        T.ProductCategoryID AS ProductCategory_ProductCategoryID, 
                        T.Name AS ProductCategory_Name, 
                        True AS _from0
                    FROM AdventureWorks.ProductCategories AS T
                ) AS T1"),
            [$"{CodeFirstDatabase}.{nameof(ProductSubcategory)}"] = new DbMappingView(@"
                SELECT VALUE -- Constructing ProductSubcategory
                    [CodeFirstDatabaseSchema.ProductSubcategory](T1.ProductSubcategory_ProductSubcategoryID, T1.ProductSubcategory_ProductCategoryID, T1.ProductSubcategory_Name)
                FROM (
                    SELECT 
                        T.ProductSubcategoryID AS ProductSubcategory_ProductSubcategoryID, 
                        T.ProductCategoryID AS ProductSubcategory_ProductCategoryID, 
                        T.Name AS ProductSubcategory_Name, 
                        True AS _from0
                    FROM AdventureWorks.ProductSubcategories AS T
                ) AS T1"),
            [$"{CodeFirstDatabase}.{nameof(Product)}"] = new DbMappingView(@"
                SELECT VALUE -- Constructing Product
                    [CodeFirstDatabaseSchema.Product](T2.Product_ProductID, T2.Product_ProductSubcategoryID, T2.Product_RowVersion, T2.Product_Name, T2.Product_ListPrice, T2.Product_Style)
                FROM (
                    SELECT -- Constructing Style
                        T1.Product_ProductID, 
                        T1.Product_ProductSubcategoryID, 
                        T1.Product_RowVersion, 
                        T1.Product_Name, 
                        T1.Product_ListPrice, 
                        CASE
                            WHEN T1._from1 THEN N'M '
                            WHEN T1._from2 THEN N'U '
                            WHEN T1._from3 THEN N'W '
                        END AS Product_Style
                    FROM (
                        SELECT 
                            T.ProductID AS Product_ProductID, 
                            T.ProductSubcategoryID AS Product_ProductSubcategoryID, 
                            T.RowVersion AS Product_RowVersion, 
                            T.Name AS Product_Name, 
                            T.ListPrice AS Product_ListPrice, 
                            True AS _from0, 
                            CASE WHEN T IS OF (ONLY [Dixin.Linq.EntityFramework.MenProduct]) THEN True ELSE False END AS _from1, 
                            CASE WHEN T IS OF (ONLY [Dixin.Linq.EntityFramework.UniversalProduct]) THEN True ELSE False END AS _from2, 
                            CASE WHEN T IS OF (ONLY [Dixin.Linq.EntityFramework.WomenProduct]) THEN True ELSE False END AS _from3
                        FROM AdventureWorks.Products AS T
                    ) AS T1
                ) AS T2"),
            [$"{CodeFirstDatabase}.{nameof(ProductPhoto)}"] = new DbMappingView(@"
                SELECT VALUE -- Constructing ProductPhoto
                    [CodeFirstDatabaseSchema.ProductPhoto](T1.ProductPhoto_ProductPhotoID, T1.ProductPhoto_ModifiedDate, T1.ProductPhoto_LargePhotoFileName)
                FROM (
                    SELECT 
                        T.ProductPhotoID AS ProductPhoto_ProductPhotoID, 
                        T.ModifiedDate AS ProductPhoto_ModifiedDate, 
                        T.LargePhotoFileName AS ProductPhoto_LargePhotoFileName, 
                        True AS _from0
                    FROM AdventureWorks.ProductPhotos AS T
                ) AS T1"),
            [$"{CodeFirstDatabase}.{nameof(ProductProductPhoto)}"] = new DbMappingView(@"
                SELECT VALUE -- Constructing ProductProductPhoto
                    [CodeFirstDatabaseSchema.ProductProductPhoto](T1.ProductProductPhoto_ProductID, T1.ProductProductPhoto_ProductPhotoID)
                FROM (
                    SELECT 
                        T.ProductID AS ProductProductPhoto_ProductID, 
                        T.ProductPhotoID AS ProductProductPhoto_ProductPhotoID, 
                        True AS _from0
                    FROM AdventureWorks.ProductProductPhotoes AS T
                ) AS T1"),

            [$"{nameof(AdventureWorks)}.{nameof(ProductCategory)}"] = new DbMappingView(@"
                SELECT VALUE -- Constructing ProductCategories
                    [Dixin.Linq.EntityFramework.ProductCategory](T1.ProductCategory_ProductCategoryID, T1.ProductCategory_Name)
                FROM (
                    SELECT 
                        T.ProductCategoryID AS ProductCategory_ProductCategoryID, 
                        T.Name AS ProductCategory_Name, 
                        True AS _from0
                    FROM CodeFirstDatabase.ProductCategory AS T
                ) AS T1"),
            [$"{nameof(AdventureWorks)}.{nameof(ProductSubcategory)}"] = new DbMappingView(@"
                SELECT VALUE -- Constructing ProductSubcategories
                    [Dixin.Linq.EntityFramework.ProductSubcategory](T1.ProductSubcategory_ProductSubcategoryID, T1.ProductSubcategory_ProductCategoryID, T1.ProductSubcategory_Name)
                FROM (
                    SELECT 
                        T.ProductSubcategoryID AS ProductSubcategory_ProductSubcategoryID, 
                        T.ProductCategoryID AS ProductSubcategory_ProductCategoryID, 
                        T.Name AS ProductSubcategory_Name, 
                        True AS _from0
                    FROM CodeFirstDatabase.ProductSubcategory AS T
                ) AS T1"),
            [$"{nameof(AdventureWorks)}.{nameof(Product)}"] = new DbMappingView(@"
                SELECT VALUE -- Constructing Products
                    CASE
                        WHEN (NOT(T1._from1) AND NOT(T1._from2) AND NOT(T1._from3)) THEN [Dixin.Linq.EntityFramework.Product](T1.Product_ProductID, T1.Product_ProductSubcategoryID, T1.Product_RowVersion, T1.Product_Name, T1.Product_ListPrice)
                        WHEN T1._from1 THEN [Dixin.Linq.EntityFramework.MenProduct](T1.Product_ProductID, T1.Product_ProductSubcategoryID, T1.Product_RowVersion, T1.Product_Name, T1.Product_ListPrice)
                        WHEN T1._from2 THEN [Dixin.Linq.EntityFramework.UniversalProduct](T1.Product_ProductID, T1.Product_ProductSubcategoryID, T1.Product_RowVersion, T1.Product_Name, T1.Product_ListPrice)
                        ELSE [Dixin.Linq.EntityFramework.WomenProduct](T1.Product_ProductID, T1.Product_ProductSubcategoryID, T1.Product_RowVersion, T1.Product_Name, T1.Product_ListPrice)
                    END
                FROM (
                    SELECT 
                        T.ProductID AS Product_ProductID, 
                        T.ProductSubcategoryID AS Product_ProductSubcategoryID, 
                        T.RowVersion AS Product_RowVersion, 
                        T.Name AS Product_Name, 
                        T.ListPrice AS Product_ListPrice, 
                        True AS _from0, 
                        CASE WHEN T.Style = N'M ' THEN True ELSE False END AS _from1, 
                        CASE WHEN T.Style = N'U ' THEN True ELSE False END AS _from2, 
                        CASE WHEN T.Style = N'W ' THEN True ELSE False END AS _from3
                    FROM CodeFirstDatabase.Product AS T
                ) AS T1"),
            [$"{nameof(AdventureWorks)}.{nameof(ProductPhoto)}"] = new DbMappingView(@"
                SELECT VALUE -- Constructing ProductPhotos
                    [Dixin.Linq.EntityFramework.ProductPhoto](T1.ProductPhoto_ProductPhotoID, T1.ProductPhoto_ModifiedDate, T1.ProductPhoto_LargePhotoFileName)
                FROM (
                    SELECT 
                        T.ProductPhotoID AS ProductPhoto_ProductPhotoID, 
                        T.ModifiedDate AS ProductPhoto_ModifiedDate, 
                        T.LargePhotoFileName AS ProductPhoto_LargePhotoFileName, 
                        True AS _from0
                    FROM CodeFirstDatabase.ProductPhoto AS T
                ) AS T1"),
            [$"{nameof(AdventureWorks)}.{nameof(ProductProductPhoto)}"] = new DbMappingView(@"
                SELECT VALUE -- Constructing ProductProductPhotoes
                    [Dixin.Linq.EntityFramework.ProductProductPhoto](T1.ProductProductPhoto_ProductID, T1.ProductProductPhoto_ProductPhotoID)
                FROM (
                    SELECT 
                        T.ProductID AS ProductProductPhoto_ProductID, 
                        T.ProductPhotoID AS ProductProductPhoto_ProductPhotoID, 
                        True AS _from0
                    FROM CodeFirstDatabase.ProductProductPhoto AS T
                ) AS T1")
        };

        public override string MappingHashValue { get; } =
            "8b3aa7066a4110e57367047f814c9511f8e1a699ae4c3177fd1dfecbe559ce86";

        public override DbMappingView GetView(EntitySetBase extent)
        {
            if (extent?.BuiltInTypeKind != BuiltInTypeKind.EntitySet)
            {
                return null;
            }

            string fullName = $"{extent.EntityContainer.Name}.{extent.ElementType.Name}";
            DbMappingView view;
            return Views.TryGetValue(fullName, out view) ? view : null;
        }
    }
}