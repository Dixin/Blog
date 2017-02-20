namespace Tutorial.LinqToEntities
{
#if EF
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
#else
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Microsoft.EntityFrameworkCore;
#endif

    [Table(nameof(vEmployee), Schema = AdventureWorks.HumanResources)]
    public class vEmployee
    {
        [Key]
        public int BusinessEntityID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string JobTitle { get; set; }

        // Other columns.
    }

    public partial class AdventureWorks
    {
        public DbSet<vEmployee> vEmployees { get; set; }
    }
}
