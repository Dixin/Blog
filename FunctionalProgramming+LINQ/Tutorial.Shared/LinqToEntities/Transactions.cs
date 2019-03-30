namespace Tutorial.LinqToEntities
{
#if EF
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Transactions;
    
    using IDbContextTransaction = System.Data.Entity.DbContextTransaction;
    using IsolationLevel = System.Data.IsolationLevel;
#else
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;

    using IsolationLevel = System.Data.IsolationLevel;
#endif

    internal static partial class Transactions
    {
        internal static void ExecutionStrategy2(AdventureWorks adventureWorks)
        {
            adventureWorks.Database.CreateExecutionStrategy().Execute(() =>
            {
                // Single retry operation, which can have custom transactions.
            });
        }
    }

    internal static partial class Transactions
    {
        internal static void Default(AdventureWorks adventureWorks)
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
                exception.WriteLine();
                // System.Data.Entity.Infrastructure.DbUpdateException: An error occurred while updating the entries. See the inner exception for details.
                // ---> System.Data.Entity.Core.UpdateException: An error occurred while updating the entries. See the inner exception for details. 
                // ---> System.Data.SqlClient.SqlException: The UPDATE statement conflicted with the FOREIGN KEY constraint "FK_ProductSubcategory_ProductCategory_ProductCategoryID". The conflict occurred in database "D:\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF", table "Production.ProductCategory", column 'ProductCategoryID'. The statement has been terminated.
                adventureWorks.Entry(category).Reload();
                category.Name.WriteLine(); // Accessories
                adventureWorks.Entry(subcategory).Reload();
                subcategory.ProductCategoryID.WriteLine(); // 1
            }
        }
    }

    public static partial class DbContextExtensions
    {
        public static readonly string CurrentIsolationLevelSql = $@"
            SELECT
                CASE transaction_isolation_level
                    WHEN 0 THEN N'{IsolationLevel.Unspecified}'
                    WHEN 1 THEN N'{IsolationLevel.ReadUncommitted}'
                    WHEN 2 THEN N'{IsolationLevel.ReadCommitted}'
                    WHEN 3 THEN N'{IsolationLevel.RepeatableRead}'
                    WHEN 4 THEN N'{IsolationLevel.Serializable}'
                    WHEN 5 THEN N'{IsolationLevel.Snapshot}'
                END
            FROM sys.dm_exec_sessions
            WHERE session_id = @@SPID";

#if EF
        public static string CurrentIsolationLevel(this DbContext context) =>
            context.Database.SqlQuery<string>(CurrentIsolationLevelSql).Single();
#else
        public static string CurrentIsolationLevel(this DbContext context)
        {
            using (DbCommand command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = CurrentIsolationLevelSql;
                command.Transaction = context.Database.CurrentTransaction.GetDbTransaction();
                return (string)command.ExecuteScalar();
            }
        }
#endif
    }

    internal static partial class Transactions
    {
        internal static void DbContextTransaction(AdventureWorks adventureWorks)
        {
            adventureWorks.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (IDbContextTransaction transaction = adventureWorks.Database.BeginTransaction(
                    IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        adventureWorks.CurrentIsolationLevel().WriteLine(); // ReadUncommitted

                        ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                        adventureWorks.ProductCategories.Add(category);
                        adventureWorks.SaveChanges().WriteLine(); // 1

                        adventureWorks.Database.ExecuteSqlCommand(
                            sql: "DELETE FROM [Production].[ProductCategory] WHERE [Name] = {0}",
                            parameters: nameof(ProductCategory)).WriteLine(); // 1
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }
    }

    internal static partial class Transactions
    {
        internal static void DbTransaction()
        {
            using (DbConnection connection = new SqlConnection(ConnectionStrings.AdventureWorks))
            {
                connection.Open();
                using (DbTransaction transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead))
                {
                    try
                    {
                        using (AdventureWorks adventureWorks = new AdventureWorks(connection))
                        {
                            adventureWorks.Database.CreateExecutionStrategy().Execute(() =>
                            {
                                adventureWorks.Database.UseTransaction(transaction);
                                adventureWorks.CurrentIsolationLevel().WriteLine(); // RepeatableRead

                                ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                                adventureWorks.ProductCategories.Add(category);
                                adventureWorks.SaveChanges().WriteLine(); // 1.
                            });
                        }
                        using (DbCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM [Production].[ProductCategory] WHERE [Name] = @p0";
                            DbParameter parameter = command.CreateParameter();
                            parameter.ParameterName = "@p0";
                            parameter.Value = nameof(ProductCategory);
                            command.Parameters.Add(parameter);
                            command.Transaction = transaction;
                            command.ExecuteNonQuery().WriteLine(); // 1
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

#if EF
        // https://github.com/dotnet/corefx/issues/22484
        // SqlClient transaction support was added recently in 6de0d94
        // It will be included in the 2.1.0 release.
        internal static void TransactionScope()
        {
            new ExecutionStrategy().Execute(() =>
            {
                using (TransactionScope scope = new TransactionScope(
                    scopeOption: TransactionScopeOption.Required,
                    transactionOptions: new TransactionOptions()
                    {
                        IsolationLevel = System.Transactions.IsolationLevel.Serializable
                    }))
                {
                    using (DbConnection connection = new SqlConnection(ConnectionStrings.AdventureWorks))
                    using (DbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = DbContextExtensions.CurrentIsolationLevelSql;
                        connection.Open();
                        using (DbDataReader reader = command.ExecuteReader())
                        {
                            reader.Read();
                            reader[0].WriteLine(); // RepeatableRead
                        }
                    }

                    using (AdventureWorks adventureWorks = new AdventureWorks())
                    {
                        ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                        adventureWorks.ProductCategories.Add(category);
                        adventureWorks.SaveChanges().WriteLine(); // 1
                    }

                    using (AdventureWorks adventureWorks = new AdventureWorks())
                    {
                        adventureWorks.CurrentIsolationLevel().WriteLine(); // Serializable
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
                        command.ExecuteNonQuery().WriteLine(); // 1
                    }

                    scope.Complete();
                }
            });
        }
#endif
    }
}