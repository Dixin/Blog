namespace Tutorial.LinqToSql
{
    using System.Diagnostics;
    using System.Linq;

    internal static class Changes
    {
        internal static int Create()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = new ProductCategory() { Name = "Category" };
                ProductSubcategory subcategory = new ProductSubcategory() { Name = "Subcategory" };
                category.ProductSubcategories.Add(subcategory);
                adventureWorks.ProductCategories.InsertOnSubmit(category);

                Trace.WriteLine(category.ProductCategoryID); // 0.
                Trace.WriteLine(subcategory.ProductCategoryID); // null.
                Trace.WriteLine(subcategory.ProductSubcategoryID); // 0.

                adventureWorks.SubmitChanges();

                Trace.WriteLine(category.ProductCategoryID); // 5.
                Trace.WriteLine(subcategory.ProductCategoryID); // 5.
                Trace.WriteLine(subcategory.ProductSubcategoryID); // 38.
                return subcategory.ProductSubcategoryID;
            }
        }

        internal static void Update()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories.First();
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories
                    .Single(entity => entity.Name == "Subcategory");
                Trace.WriteLine(subcategory.Name); // Subcategory.
                Trace.WriteLine(subcategory.ProductCategoryID); // 5.

                subcategory.Name = "Update"; // Update property.
                subcategory.ProductCategory = category; // Update association.

                adventureWorks.SubmitChanges();

                Trace.WriteLine(subcategory.Name); // Subcategory update.
                Trace.WriteLine(subcategory.ProductCategoryID); // 4.
            }
        }

        internal static void UpdateWithNoChange()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Product product = adventureWorks.Find<Product>(999);
                product.ListPrice += product.ListPrice;
                product.ListPrice /= 2; // Change tracked entity then change back.
                Trace.WriteLine(adventureWorks.GetChangeSet().Updates.Any()); // False.
                adventureWorks.SubmitChanges();
            }
        }

        internal static void Delete()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories
                    .Single(entity => entity.Name == "Category");
                adventureWorks.ProductCategories.DeleteOnSubmit(category);
                adventureWorks.SubmitChanges();
            }
        }

        internal static void DeleteWithNoQuery(int subcategoryId)
        {
            ProductSubcategory subcategory = new ProductSubcategory() { ProductSubcategoryID = subcategoryId };
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.ProductSubcategories.Attach(subcategory, false);
                adventureWorks.ProductSubcategories.DeleteOnSubmit(subcategory);
                adventureWorks.SubmitChanges();
            }
        }

        internal static void DeleteWithRelationship()
        {
            Create(); // Insert ProductCategory "Category" and ProductSubcategory "Subcategory".
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories
                    .Single(entity => entity.Name == "Category");
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories
                    .Single(entity => entity.Name == "Subcategory");
                adventureWorks.ProductCategories.DeleteOnSubmit(category);
                adventureWorks.ProductSubcategories.DeleteOnSubmit(subcategory);
                adventureWorks.SubmitChanges();
            }
        }

        internal static void UntrackedChanges()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.ObjectTrackingEnabled = false;
                IQueryable<Product> products = adventureWorks.Products.Take(10);
                adventureWorks.Products.DeleteAllOnSubmit(products);
                adventureWorks.SubmitChanges();
                // InvalidOperationException: Object tracking is not enabled for the current data context instance.
            }
        }
    }
}
