namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
#if NETFX
    using System.Data.Entity;
#endif
    using System.Reflection;

#if !NETFX
    using Microsoft.EntityFrameworkCore;
#endif

    [Table(nameof(WideWorldImporters.Countries), Schema = WideWorldImporters.Application)]
    public abstract class Country
    {
        [Key]
        public int CountryID { get; set; }

        public string CountryName { get; set; }

        public long LatestRecordedPopulation { get; set; }
    }

    public partial class WideWorldImporters
    {
        public const string Application = nameof(Application);

        public DbSet<Country> Countries { get; set; }
    }

    public class AfricaCountry : Country { }

    public class AmericasCountry : Country { }

    public class AsiaCountry : Country { }

    public class EuropeCountry : Country { }

    public class OceaniaeCountry : Country { }

    public enum Region
    {
        Africa,
        Americas,
        Asia,
        Europe,
        Oceania
    }

    public partial class WideWorldImporters
    {
#if NETFX
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Country>()
                .Map<AfricaCountry>(mapping => mapping.Requires(nameof(Region)).HasValue(nameof(Region.Africa)))
                .Map<AmericasCountry>(mapping => mapping.Requires(nameof(Region)).HasValue(nameof(Region.Americas)))
                .Map<AsiaCountry>(mapping => mapping.Requires(nameof(Region)).HasValue(nameof(Region.Asia)))
                .Map<EuropeCountry>(mapping => mapping.Requires(nameof(Region)).HasValue(nameof(Region.Europe)))
                .Map<OceaniaeCountry>(mapping => mapping.Requires(nameof(Region)).HasValue(nameof(Region.Oceania)));
        }
#else
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>()
                .HasDiscriminator<string>(nameof(Region))
                .HasValue<AfricaCountry>(nameof(Region.Africa))
                .HasValue<AmericasCountry>(nameof(Region.Americas))
                .HasValue<AsiaCountry>(nameof(Region.Asia))
                .HasValue<EuropeCountry>(nameof(Region.Europe))
                .HasValue<OceaniaeCountry>(nameof(Region.Oceania));

            modelBuilder.Entity<StockItemStockGroup>()
                .HasOne(productProductPhoto => productProductPhoto.StockItem)
                .WithMany(product => product.StockItemStockGroups)
                .HasForeignKey(productProductPhoto => productProductPhoto.StockItemID);

            modelBuilder.Entity<StockItemStockGroup>()
                .HasOne(productProductPhoto => productProductPhoto.StockGroup)
                .WithMany(photo => photo.StockItemStockGroups)
                .HasForeignKey(productProductPhoto => productProductPhoto.StockGroupID);

            modelBuilder.SequenceKey(
                Sequences, typeof(SupplierCategory), typeof(Supplier), typeof(Country), typeof(StockItem), typeof(StockItemHolding), typeof(StockItemStockGroup), typeof(StockGroup));
        }
#endif
    }

#if !NETFX
    public static class ModelBuilerExtensions
    {
        public static ModelBuilder SequenceKey(this ModelBuilder modelBuilder, string sequenceSchema, params Type[] entityTypes)
        {
            entityTypes.ForEach(entityType =>
            {
                PropertyInfo key = entityType.GetTypeInfo().GetProperties().Single(property => property.IsDefined(typeof(KeyAttribute)));

                modelBuilder.ForSqlServerHasSequence<int>(key.Name, sequenceSchema)
                    .StartsAt(1).IncrementsBy(1);
                modelBuilder.Entity(entityType)
                    .Property(key.Name)
                    .HasDefaultValueSql($"NEXT VALUE FOR [{sequenceSchema}].[{key.Name}]");
            });
            return modelBuilder;
        }
    }
#endif
}
