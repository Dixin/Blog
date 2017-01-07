namespace Dixin.Tests.Linq.EntityFramework
{
    using System;
    using System.Data.Common;
#if NETFX
    using System.Data.Entity.Infrastructure;
#endif
    using System.Data.SqlClient;
    using System.Diagnostics;
#if NETFX
    using System.Transactions;
#endif
    using Dixin.Linq;
    using Dixin.Linq.EntityFramework;

#if !NETFX
    using Microsoft.EntityFrameworkCore;
#endif
    using Microsoft.VisualStudio.TestTools.UnitTesting;

#if !NETFX
    using static TransactionHelper;

    internal static class TransactionHelper
    {
        internal static string DefaultConnection { get; set; } = ConnectionStrings.AdventureWorks;

        internal static void Rollback(Action<WideWorldImporters> action, string connectionString = null)
        {
            using (DbConnection connection = new SqlConnection(connectionString ?? DefaultConnection))
            {
                connection.Open();
                using (WideWorldImporters adventureWorks1 = new WideWorldImporters(connection))
                {
                    adventureWorks1.Database.CreateExecutionStrategy().Execute(() =>
                    {
                        using (DbTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                adventureWorks1.Database.UseTransaction(transaction);
                                action(adventureWorks1);
                            }
                            finally
                            {
                                transaction.Rollback();
                            }
                        }
                    });
                }
            }
        }

        internal static void Rollback(Action<WideWorldImporters, WideWorldImporters> action, string connectionString = null)
        {
            using (DbConnection connection = new SqlConnection(connectionString ?? DefaultConnection))
            {
                connection.Open();
                using (WideWorldImporters adventureWorks1 = new WideWorldImporters(connection))
                using (WideWorldImporters adventureWorks2 = new WideWorldImporters(connection))
                {
                    adventureWorks1.Database.CreateExecutionStrategy().Execute(() =>
                    {
                        adventureWorks2.Database.CreateExecutionStrategy().Execute(() =>
                        {
                            using (DbTransaction transaction = connection.BeginTransaction())
                            {
                                try
                                {
                                    adventureWorks1.Database.UseTransaction(transaction);
                                    adventureWorks2.Database.UseTransaction(transaction);
                                    action(adventureWorks1, adventureWorks2);
                                }
                                finally
                                {
                                    transaction.Rollback();
                                }
                            }
                        });
                    });
                }
            }
        }

        internal static void Rollback(Action<WideWorldImporters, WideWorldImporters, WideWorldImporters> action, string connectionString = null)
        {
            using (DbConnection connection = new SqlConnection(connectionString ?? DefaultConnection))
            {
                connection.Open();
                using (WideWorldImporters adventureWorks1 = new WideWorldImporters(connection))
                using (WideWorldImporters adventureWorks2 = new WideWorldImporters(connection))
                using (WideWorldImporters adventureWorks3 = new WideWorldImporters(connection))
                {
                    adventureWorks1.Database.CreateExecutionStrategy().Execute(() =>
                    {
                        adventureWorks2.Database.CreateExecutionStrategy().Execute(() =>
                        {
                            adventureWorks3.Database.CreateExecutionStrategy().Execute(() =>
                            {
                                using (DbTransaction transaction = connection.BeginTransaction())
                                {
                                    try
                                    {
                                        adventureWorks1.Database.UseTransaction(transaction);
                                        adventureWorks2.Database.UseTransaction(transaction);
                                        adventureWorks3.Database.UseTransaction(transaction);
                                        action(adventureWorks1, adventureWorks2, adventureWorks3);
                                    }
                                    finally
                                    {
                                        transaction.Rollback();
                                    }
                                }
                            });
                        });
                    });
                }
            }
        }

        internal static void Rollback(Action<WideWorldImporters, WideWorldImporters, WideWorldImporters, WideWorldImporters> action, string connectionString = null)
        {
            using (DbConnection connection = new SqlConnection(connectionString ?? DefaultConnection))
            {
                connection.Open();
                using (WideWorldImporters adventureWorks1 = new WideWorldImporters(connection))
                using (WideWorldImporters adventureWorks2 = new WideWorldImporters(connection))
                using (WideWorldImporters adventureWorks3 = new WideWorldImporters(connection))
                using (WideWorldImporters adventureWorks4 = new WideWorldImporters(connection))
                {
                    adventureWorks1.Database.CreateExecutionStrategy().Execute(() =>
                    {
                        adventureWorks2.Database.CreateExecutionStrategy().Execute(() =>
                        {
                            adventureWorks3.Database.CreateExecutionStrategy().Execute(() =>
                            {
                                adventureWorks4.Database.CreateExecutionStrategy().Execute(() =>
                                {
                                    using (DbTransaction transaction = connection.BeginTransaction())
                                    {
                                        try
                                        {
                                            adventureWorks1.Database.UseTransaction(transaction);
                                            adventureWorks2.Database.UseTransaction(transaction);
                                            adventureWorks3.Database.UseTransaction(transaction);
                                            adventureWorks4.Database.UseTransaction(transaction);
                                            action(adventureWorks1, adventureWorks2, adventureWorks3, adventureWorks4);
                                        }
                                        finally
                                        {
                                            transaction.Rollback();
                                        }
                                    }
                                });
                            });
                        });
                    });
                }
            }
        }
    }
#endif

    [TestClass]
    public class ConcurrencyTests
    {
        [TestMethod]
        public void DetectConflictTest()
        {
#if NETFX
            using (new TransactionScope())
            {
                Concurrency.NoCheck(new WideWorldImporters(), new WideWorldImporters(), new WideWorldImporters());
            }
            using (new TransactionScope())
            {
                try
                {
                    Concurrency.ConcurrencyCheck(new WideWorldImporters(), new WideWorldImporters());
                    Assert.Fail();
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    Trace.WriteLine(exception);
                }
            }
            using (new TransactionScope())
            {
                try
                {
                    Concurrency.RowVersion(new WideWorldImporters(), new WideWorldImporters());
                    Assert.Fail();
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    Trace.WriteLine(exception);
                }
            }
#else
            Rollback((adventureWorks1, adventureWorks2, adventureWorks3) => Concurrency.NoCheck(adventureWorks1, adventureWorks2, adventureWorks3));
            Rollback((adventureWorks1, adventureWorks2) =>
            {
                try
                {
                    Concurrency.ConcurrencyCheck(adventureWorks1, adventureWorks2);
                    Assert.Fail();
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    Trace.WriteLine(exception);
                }
            });
            Rollback((adventureWorks1, adventureWorks2) =>
            {
                try
                {
                    Concurrency.RowVersion(adventureWorks1, adventureWorks2);
                    Assert.Fail();
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    Trace.WriteLine(exception);
                }
            });
#endif
        }

        [TestMethod]
        public void UpdateConflictTest()
        {
#if NETFX
            using (new TransactionScope())
            {
                Concurrency.UpdateProductDatabaseWins(new WideWorldImporters(), new WideWorldImporters(), new WideWorldImporters());
            }
            using (new TransactionScope())
            {
                Concurrency.UpdateProductClientWins(new WideWorldImporters(), new WideWorldImporters(), new WideWorldImporters());
            }
            using (new TransactionScope())
            {
                Concurrency.UpdateProductMergeClientAndDatabase(new WideWorldImporters(), new WideWorldImporters(), new WideWorldImporters());
            }
            using (new TransactionScope())
            {
                Concurrency.SaveChanges(new WideWorldImporters(), new WideWorldImporters());
            }
#else
            Rollback((adventureWorks1, adventureWorks2, adventureWorks3) => 
                Concurrency.UpdateProductDatabaseWins(adventureWorks1, adventureWorks2, adventureWorks3));
            Rollback((adventureWorks1, adventureWorks2, adventureWorks3) => 
                Concurrency.UpdateProductClientWins(adventureWorks1, adventureWorks2, adventureWorks3));
            Rollback((adventureWorks1, adventureWorks2, adventureWorks3) => 
                Concurrency.UpdateProductMergeClientAndDatabase(adventureWorks1, adventureWorks2, adventureWorks3));
            Rollback((adventureWorks1, adventureWorks2) => 
                Concurrency.SaveChanges(adventureWorks1, adventureWorks2));
#endif
        }
    }
}
