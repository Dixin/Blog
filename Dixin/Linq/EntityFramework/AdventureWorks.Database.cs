namespace Dixin.Linq.EntityFramework
{
    using System.Data.Entity;

    public partial class AdventureWorks : DbContext
    {
        public AdventureWorks()
            : base(ConnectionStrings.AdventureWorks)
        {
        }
    }
}
