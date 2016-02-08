namespace Dixin.Linq.LinqToSql
{
    using System.Data.Linq;
    using System.Data.Linq.Mapping;

    [Database(Name = "[AdventureWorks]")]
    public partial class AdventureWorks : DataContext
    {
        public AdventureWorks()
            : base(Linq.Connection.String)
        {
            // if (!this.DatabaseExists())
            // {
            //    this.CreateDatabase();
            // }
        }
    }
}
