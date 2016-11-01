namespace Dixin.Linq.EntityFramework
{
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Transactions;

    using IsolationLevel = System.Data.IsolationLevel;

    internal static partial class Transactions
    {
        internal static void Default()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories.First();
                category.Name = "Update"; // Valid value.g
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First();
                subcategory.ProductCategoryID = -1; // Invalid value.
                try
                {
                    adventureWorks.SaveChanges();
                }
                catch (DbUpdateException exception)
                {
                    Trace.WriteLine(exception);
                    // System.Data.Entity.Infrastructure.DbUpdateException: An error occurred while updating the entries. See the inner exception for details.
                    // ---> System.Data.Entity.Core.UpdateException: An error occurred while updating the entries. See the inner exception for details. 
                    // ---> System.Data.SqlClient.SqlException: The UPDATE statement conflicted with the FOREIGN KEY constraint "FK_ProductSubcategory_ProductCategory_ProductCategoryID". The conflict occurred in database "D:\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF", table "Production.ProductCategory", column 'ProductCategoryID'. The statement has been terminated.
                    adventureWorks.Entry(category).Reload();
                    Trace.WriteLine(category.Name); // Accessories
                    adventureWorks.Entry(subcategory).Reload();
                    Trace.WriteLine(subcategory.ProductCategoryID); // 1
                }
            }
        }
    }

    public static partial class DbContextExtensions
    {
        public const string CurrentIsolationLevelSql = @"
            SELECT
                CASE transaction_isolation_level
                    WHEN 0 THEN N'Unspecified'
                    WHEN 1 THEN N'ReadUncommitted'
                    WHEN 2 THEN N'ReadCommitted'
                    WHEN 3 THEN N'RepeatableRead'
                    WHEN 4 THEN N'Serializable'
                    WHEN 5 THEN N'Snapshot'
                END
            FROM sys.dm_exec_sessions
            WHERE session_id = @@SPID";

        public static string QueryCurrentIsolationLevel(this DbContext context) => 
            context.Database.SqlQuery<string>(CurrentIsolationLevelSql).Single();
    }

    internal static partial class Transactions
    {
        internal static void DbContextTransaction()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            using (DbContextTransaction transaction = adventureWorks.Database.BeginTransaction(
                IsolationLevel.ReadUncommitted))
            {
                try
                {
                    Trace.WriteLine(adventureWorks.QueryCurrentIsolationLevel()); // ReadUncommitted

                    ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                    adventureWorks.ProductCategories.Add(category);
                    Trace.WriteLine(adventureWorks.SaveChanges()); // 1

                    Trace.WriteLine(adventureWorks.Database.ExecuteSqlCommand(
                        "DELETE FROM [Production].[ProductCategory] WHERE [Name] = {0}",
                        nameof(ProductCategory))); // 1
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }

    public partial class AdventureWorks
    {
        public AdventureWorks(DbConnection connection, bool contextOwnsConnection = false)
            : base(connection, contextOwnsConnection)
        {
        }
    }

    internal static partial class Transactions
    {
        internal static void DbTransaction()
        {
            using (DbConnection connection = new SqlConnection(ConnectionStrings.AdventureWorks))
            {
                connection.Open();
                using (DbTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        using (AdventureWorks adventureWorks = new AdventureWorks(connection))
                        {
                            adventureWorks.Database.UseTransaction(transaction);
                            Trace.WriteLine(adventureWorks.QueryCurrentIsolationLevel()); // Serializable

                            ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                            adventureWorks.ProductCategories.Add(category);
                            Trace.WriteLine(adventureWorks.SaveChanges()); // 1.
                        }

                        using (DbCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM [Production].[ProductCategory] WHERE [Name] = @p0";
                            DbParameter parameter = command.CreateParameter();
                            parameter.ParameterName = "@p0";
                            parameter.Value = nameof(ProductCategory);
                            command.Parameters.Add(parameter);
                            command.Transaction = transaction;
                            Trace.WriteLine(command.ExecuteNonQuery()); // 1
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        internal static void TransactionScope()
        {
            using (TransactionScope scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead }))
            {
                using (DbConnection connection = new SqlConnection(ConnectionStrings.AdventureWorks))
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = DbContextExtensions.CurrentIsolationLevelSql;
                    connection.Open();
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        Trace.WriteLine(reader[0]); // RepeatableRead
                    }
                }

                using (AdventureWorks adventureWorks = new AdventureWorks())
                {
                    ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                    adventureWorks.ProductCategories.Add(category);
                    Trace.WriteLine(adventureWorks.SaveChanges()); // 1
                }

                using (DbConnection connection = new SqlConnection(ConnectionStrings.AdventureWorks))
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM [Production].[ProductCategory] WHERE [Name] = @p0";
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@p0";
                    parameter.Value = nameof(ProductCategory);
                    command.Parameters.Add(parameter);

                    connection.Open();
                    Trace.WriteLine(command.ExecuteNonQuery()); // 1
                }

                scope.Complete();
            }
        }
    }
}