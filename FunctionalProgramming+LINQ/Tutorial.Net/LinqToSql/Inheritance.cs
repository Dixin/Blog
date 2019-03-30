namespace Tutorial.LinqToSql
{
    using System.Data.Linq.Mapping;

    [InheritanceMapping(Code = "U ", Type = typeof(UniversalProduct), IsDefault = true)]
    [InheritanceMapping(Code = "W ", Type = typeof(WomenProduct))]
    [InheritanceMapping(Code = "M ", Type = typeof(MenProduct))]
    public partial class Product
    {
        [Column(DbType = "nchar(2)", IsDiscriminator = true)]
        public string Style { get; set; }
    }

    public class WomenProduct : Product { }

    public class MenProduct : Product { }

    public class UniversalProduct : Product { }
}
