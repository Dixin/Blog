namespace Tutorial.Tests.LinqToEntities
{
    using System;
    using System.Data.Common;
#if NETFX
    using System.Data.Entity.Infrastructure;
#endif
    using System.Data.SqlClient;
#if NETFX
    using System.Transactions;
#endif

    using Tutorial;
    using Tutorial.LinqToEntities;

#if !NETFX
    using Microsoft.EntityFrameworkCore;
#endif

#if NETFX
    internal class TransactionHelper : IDisposable
    {
        private readonly TransactionScope transaction;

        internal TransactionHelper(TransactionScopeAsyncFlowOption option = TransactionScopeAsyncFlowOption.Enabled)
        {
            ExecutionStrategy.DisableExecutionStrategy = true;
            this.transaction = new TransactionScope(option);
        }

        public void Dispose()
        {
            this.transaction.Dispose();
            ExecutionStrategy.DisableExecutionStrategy = false;
        }
    }
#else

    internal static class TransactionHelper
    {
        internal static string DefaultConnection { get; set; } = ConnectionStrings.AdventureWorks;

        internal static void Rollback(Action<AdventureWorks> action, string connectionString = null)
        {
            using (DbConnection connection = new SqlConnection(connectionString ?? DefaultConnection))
            {
                connection.Open();
                using (AdventureWorks adventureWorks1 = new AdventureWorks(connection))
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

        internal static void Rollback(Action<AdventureWorks, AdventureWorks> action, string connectionString = null)
        {
            using (DbConnection connection = new SqlConnection(connectionString ?? DefaultConnection))
            {
                connection.Open();
                using (AdventureWorks adventureWorks1 = new AdventureWorks(connection))
                using (AdventureWorks adventureWorks2 = new AdventureWorks(connection))
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

        internal static void Rollback(Action<AdventureWorks, AdventureWorks, AdventureWorks> action, string connectionString = null)
        {
            using (DbConnection connection = new SqlConnection(connectionString ?? DefaultConnection))
            {
                connection.Open();
                using (AdventureWorks adventureWorks1 = new AdventureWorks(connection))
                using (AdventureWorks adventureWorks2 = new AdventureWorks(connection))
                using (AdventureWorks adventureWorks3 = new AdventureWorks(connection))
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

        internal static void Rollback(Action<AdventureWorks, AdventureWorks, AdventureWorks, AdventureWorks> action, string connectionString = null)
        {
            using (DbConnection connection = new SqlConnection(connectionString ?? DefaultConnection))
            {
                connection.Open();
                using (AdventureWorks adventureWorks1 = new AdventureWorks(connection))
                using (AdventureWorks adventureWorks2 = new AdventureWorks(connection))
                using (AdventureWorks adventureWorks3 = new AdventureWorks(connection))
                using (AdventureWorks adventureWorks4 = new AdventureWorks(connection))
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
}
