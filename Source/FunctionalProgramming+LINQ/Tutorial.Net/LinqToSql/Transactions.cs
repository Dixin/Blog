namespace Tutorial.LinqToSql
{
    using System;
    using System.Data.Common;
    using System.Data.Linq;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Transactions;

    internal static class Transactions
    {
        internal static void Default()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories.First();
                Trace.WriteLine(category.Name); // Accessories.
                category.Name = "Update";
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First();
                Trace.WriteLine(subcategory.ProductCategoryID); // 1.
                subcategory.ProductCategoryID = -1;
                try
                {
                    adventureWorks.SubmitChanges();
                }
                catch (SqlException exception)
                {
                    Trace.WriteLine(exception);
                    // SqlException: The UPDATE statement conflicted with the FOREIGN KEY constraint "FK_ProductSubcategory_ProductCategory_ProductCategoryID". The conflict occurred in database "D:\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF", table "Production.ProductCategory", column 'ProductCategoryID'. The statement has been terminated.
                    adventureWorks.Refresh(RefreshMode.OverwriteCurrentValues, category, subcategory);
                    Trace.WriteLine(category.Name); // Accessories.
                    Trace.WriteLine(subcategory.ProductCategoryID); // 1.
                }
            }
        }

        internal static void DbTransaction()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            using (DbConnection connection = adventureWorks.Connection)
            {
                connection.Open();
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        adventureWorks.Transaction = transaction;
                        ProductCategory category = new ProductCategory() { Name = "Transaction" };
                        adventureWorks.ProductCategories.InsertOnSubmit(category);
                        adventureWorks.SubmitChanges();
                        using (DbCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM [Production].[ProductCategory] WHERE [Name] = N'Transaction'";
                            command.Transaction = transaction;
                            Trace.WriteLine(command.ExecuteNonQuery()); // 1.
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        internal static void TransactionScope()
        {
            using (TransactionScope scope = new TransactionScope())
            using (AdventureWorks adventureWorks = new AdventureWorks())
            using (DbConnection connection = adventureWorks.Connection)
            {
                connection.Open();
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO [Production].[ProductCategory] ([Name]) VALUES (N'Transaction')";
                    Trace.WriteLine(command.ExecuteNonQuery()); // 1.
                }
                ProductCategory category = adventureWorks.ProductCategories.Single(entity => entity.Name == "Transaction");
                adventureWorks.ProductCategories.DeleteOnSubmit(category);
                adventureWorks.SubmitChanges();
                scope.Complete();
            }
        }
    }
}
