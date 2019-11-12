namespace Tutorial.LinqToSql
{
    using System.Data.Linq;
    using System.Diagnostics;
    using System.Linq;

    internal static class Tracking
    {
        internal static void EntitiesFromSameDataContext()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Product productById = adventureWorks.Products.Single(product => product.ProductID == 999);
                Product productByName = adventureWorks.Products.Single(product => product.Name == "Road-750 Black, 52");
                Trace.WriteLine(object.ReferenceEquals(productById, productByName)); // True.
            }
        }

        internal static void ObjectsFromSameDataContext()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                var productById = adventureWorks.Products
                    .Where(product => product.ProductID == 999)
                    .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                    .Single();
                var productByName = adventureWorks.Products
                    .Where(product => product.Name == "Road-750 Black, 52")
                    .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                    .Single();
                Trace.WriteLine(object.ReferenceEquals(productById, productByName)); // False.
            }
        }

        internal static void EntitiesFromDataContexts()
        {
            Product productById;
            Product productByName;
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                productById = adventureWorks.Products.Single(product => product.ProductID == 999);
            }
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                productByName = adventureWorks.Products.Single(product => product.Name == "Road-750 Black, 52");
            }
            Trace.WriteLine(object.ReferenceEquals(productById, productByName)); // False.
        }

        internal static void EntityChanges()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Product insert = new UniversalProduct() { Name = "Insert", ListPrice = 123 };
                // Product insert = new Product() causes InvalidOperationException for InsertOnSubmit:
                // InvalidOperationException: Instance of type 'Tutorial.LinqToSql.Product' could not be added. This type is not part of the mapped type system.
                adventureWorks.Products.InsertOnSubmit(insert);
                IQueryable<Product> update = adventureWorks.Products
                    .Where(product => product.Name.Contains("HL"));
                update.ForEach(product => product.ListPrice += 50);
                IQueryable<Product> delete = adventureWorks.Products
                    .Where(product => product.Name.Contains("ML"));
                adventureWorks.Products.DeleteAllOnSubmit(delete);

                ChangeSet changeSet = adventureWorks.GetChangeSet();
                Trace.WriteLine(changeSet.Inserts.Any()); // True.
                Trace.WriteLine(changeSet.Updates.Any()); // True.
                Trace.WriteLine(changeSet.Deletes.Any()); // True.

                changeSet.Inserts.OfType<Product>().ForEach(product => Trace.WriteLine(
                    $"{nameof(ChangeSet.Inserts)}: ({product.ProductID}, {product.Name}, {product.ListPrice})"));
                changeSet.Updates.OfType<Product>().ForEach(product =>
                {
                    Product original = adventureWorks.Products.GetOriginalEntityState(product);
                    Trace.WriteLine($"{nameof(ChangeSet.Updates)}: ({original.ProductID}, {original.Name}, {original.ListPrice}) -> ({product.ProductID}, {product.Name}, {product.ListPrice})");
                });
                changeSet.Deletes.OfType<Product>().ForEach(product => Trace.WriteLine(
                    $"{nameof(ChangeSet.Deletes)}: ({product.ProductID}, {product.Name}, {product.ListPrice})"));
                // Inserts: (0, Insert, 123)
                // Updates: (951, HL Crankset, 404.9900) -> (951, HL Crankset, 454.9900)
                // Updates: (996, HL Bottom Bracket, 121.4900) -> (996, HL Bottom Bracket, 171.4900)
                // Deletes: (950, ML Crankset, 256.4900)
                // Deletes: (995, ML Bottom Bracket, 101.2400)
            }
        }

        internal static void Attach()
        {
            Product product = new UniversalProduct() { Name = "On the fly", ListPrice = 1 };
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                product.ListPrice = 2;
                Trace.WriteLine(adventureWorks.GetChangeSet().Updates.Any()); // False.
                adventureWorks.Products.Attach(product);
                product.ListPrice = 3;
                adventureWorks.GetChangeSet().Updates.OfType<Product>().ForEach(attached => Trace.WriteLine(
                    $"{adventureWorks.Products.GetOriginalEntityState(attached).ListPrice} -> {attached.ListPrice}")); // 2 -> 3.
            }
        }

        internal static void RelationshipChanges()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                DataLoadOptions loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<ProductCategory>(entity => entity.ProductSubcategories);
                adventureWorks.LoadOptions = loadOptions;
                ProductCategory category = adventureWorks.ProductCategories.First();
                Trace.WriteLine(category.ProductSubcategories.Count); // 12.
                ProductSubcategory[] subcategories = category.ProductSubcategories.ToArray();
                Trace.WriteLine(subcategories.All(subcategory => subcategory.ProductCategory == category)); // True.

                category.ProductSubcategories.Clear();
                Trace.WriteLine(category.ProductSubcategories.Count); // 0.
                Trace.WriteLine(adventureWorks.GetChangeSet().Updates.Count); // 12.
                Trace.WriteLine(subcategories.All(subcategory => subcategory.ProductCategory == null)); // True.
            }
        }

        internal static void DisableObjectTracking()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.ObjectTrackingEnabled = false;
                Product product = adventureWorks.Products.First();
                product.ListPrice += 100;
                Trace.WriteLine(adventureWorks.GetChangeSet().Updates.Any()); // False
                adventureWorks.ObjectTrackingEnabled = true;
                // System.InvalidOperationException: Data context options cannot be modified after results have been returned from a query.
            }
        }
    }
}
