namespace Tutorial.LinqToEntities
{
#if EF
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    
    using ModelBuilder = System.Data.Entity.DbModelBuilder;
#else
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Microsoft.EntityFrameworkCore;
#endif

    [Table(nameof(TransactionHistory), Schema = AdventureWorks.Production)]
    public abstract class TransactionHistory
    {
        [Key]
        public int TransactionID { get; set; }

        public int ProductID { get; set; }

        public DateTime TransactionDate { get; set; }

        public int Quantity { get; set; }

        public decimal ActualCost { get; set; }
    }

    public class PurchaseTransactionHistory : TransactionHistory { }

    public class SalesTransactionHistory : TransactionHistory { }

    public class WorkTransactionHistory : TransactionHistory { }

    public enum TransactionType { P, S, W }

    public partial class AdventureWorks
    {
        private static void MapDiscriminator(ModelBuilder modelBuilder) // Called by OnModelCreating.
        {
#if EF
            modelBuilder
                .Entity<TransactionHistory>()
                .Map<PurchaseTransactionHistory>(mapping => mapping.Requires(nameof(TransactionType))
                    .HasValue(nameof(TransactionType.P)))
                .Map<SalesTransactionHistory>(mapping => mapping.Requires(nameof(TransactionType))
                    .HasValue(nameof(TransactionType.S)))
                .Map<WorkTransactionHistory>(mapping => mapping.Requires(nameof(TransactionType))
                    .HasValue(nameof(TransactionType.W)));
#else
            modelBuilder.Entity<TransactionHistory>()
                .HasDiscriminator<string>(nameof(TransactionType))
                .HasValue<PurchaseTransactionHistory>(nameof(TransactionType.P))
                .HasValue<SalesTransactionHistory>(nameof(TransactionType.S))
                .HasValue<WorkTransactionHistory>(nameof(TransactionType.W));
#endif
        }
    }

    public partial class AdventureWorks
    {
        public DbSet<TransactionHistory> Transactions { get; set; }

        public DbSet<PurchaseTransactionHistory> PurchaseTransactions { get; set; }

        public DbSet<SalesTransactionHistory> SalesTransactions { get; set; }

        public DbSet<WorkTransactionHistory> WorkTransactions { get; set; }
    }
}
