namespace Dixin.Linq.EntityFramework
{
    using System.Data.Common;
#if NETFX
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
#endif
    using System.Data.SqlClient;
    using System.Linq;

#if NETFX
    using System.Transactions;
#else

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
#endif

#if NETFX
    using IDbContextTransaction = System.Data.Entity.DbContextTransaction;
#endif
    using IsolationLevel = System.Data.IsolationLevel;

    internal static partial class Transactions
    {
        internal static void Default(WideWorldImporters adventureWorks)
        {
            SupplierCategory category = adventureWorks.SupplierCategories.First();
            category.SupplierCategoryName = "Update"; // Valid value.g
            Supplier subcategory = adventureWorks.Suppliers.First();
            subcategory.SupplierCategoryID = -1; // Invalid value.
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
                category.SupplierCategoryName.WriteLine(); // Accessories
                adventureWorks.Entry(subcategory).Reload();
                subcategory.SupplierCategoryID.WriteLine(); // 1
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

        public static string QueryCurrentIsolationLevel(this DbContext context)
        {
#if NETFX
            return context.Database.SqlQuery<string>(CurrentIsolationLevelSql).Single();
#else
            using (DbCommand command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = CurrentIsolationLevelSql;
                command.Transaction = context.Database.CurrentTransaction.GetDbTransaction();
                return (string)command.ExecuteScalar();
            }
#endif
        }
    }

    internal static partial class Transactions
    {
        internal static void DbContextTransaction(WideWorldImporters adventureWorks)
        {
#if !NETFX
            adventureWorks.Database.CreateExecutionStrategy().Execute(() =>
            {
#endif
                using (IDbContextTransaction transaction = adventureWorks.Database.BeginTransaction(
                    IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        adventureWorks.QueryCurrentIsolationLevel().WriteLine(); // ReadUncommitted

                        SupplierCategory category = new SupplierCategory() { SupplierCategoryName = nameof(SupplierCategory) };
                        adventureWorks.SupplierCategories.Add(category);
                        adventureWorks.SaveChanges().WriteLine(); // 1

                        adventureWorks.Database.ExecuteSqlCommand(
                            "DELETE FROM [Purchasing].[SupplierCategories] WHERE [SupplierCategoryName] = {0}",
                            nameof(SupplierCategory)).WriteLine(); // 1
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
#if !NETFX
            });
#endif
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
                        using (WideWorldImporters adventureWorks = new WideWorldImporters(connection))
                        {
#if !NETFX
                            adventureWorks.Database.CreateExecutionStrategy().Execute(() =>
                            {
#endif
                                adventureWorks.Database.UseTransaction(transaction);
                                adventureWorks.QueryCurrentIsolationLevel().WriteLine(); // Serializable

                                SupplierCategory category = new SupplierCategory() { SupplierCategoryName = nameof(SupplierCategory) };
                                adventureWorks.SupplierCategories.Add(category);
                                adventureWorks.SaveChanges().WriteLine(); // 1.
#if !NETFX
                            });
#endif
                        }

                        using (DbCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM [Purchasing].[SupplierCategories] WHERE [SupplierCategoryName] = @p0";
                            DbParameter parameter = command.CreateParameter();
                            parameter.ParameterName = "@p0";
                            parameter.Value = nameof(SupplierCategory);
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

#if NETFX
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
                        reader[0].WriteLine(); // RepeatableRead
                    }
                }

                using (WideWorldImporters adventureWorks = new WideWorldImporters())
                {
                    SupplierCategory category = new SupplierCategory() { SupplierCategoryName = nameof(SupplierCategory) };
                    adventureWorks.SupplierCategories.Add(category);
                    adventureWorks.SaveChanges().WriteLine(); // 1
                }

                using (DbConnection connection = new SqlConnection(ConnectionStrings.AdventureWorks))
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM [Production].[ProductCategory] WHERE [Name] = @p0";
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@p0";
                    parameter.Value = nameof(SupplierCategory);
                    command.Parameters.Add(parameter);

                    connection.Open();
                    command.ExecuteNonQuery().WriteLine(); // 1
                }

                scope.Complete();
            }
        }
#endif
    }
}