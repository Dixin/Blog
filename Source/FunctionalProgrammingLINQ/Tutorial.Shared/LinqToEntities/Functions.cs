namespace Tutorial.LinqToEntities
{
#if EF
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;

    [ComplexType]
    public class ManagerEmployee
    {
        public int? RecursionLevel { get; set; }

        public string OrganizationNode { get; set; }

        public string ManagerFirstName { get; set; }

        public string ManagerLastName { get; set; }

        public int? BusinessEntityID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    public partial class AdventureWorks
    {
        public ObjectResult<ManagerEmployee> GetManagerEmployees(int businessEntityId)
        {
            const string BusinessEntityID = nameof(BusinessEntityID);
            SqlParameter businessEntityIdParameter = new SqlParameter(nameof(BusinessEntityID), businessEntityId);
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<ManagerEmployee>(
                $"[dbo].[uspGetManagerEmployees] @{nameof(BusinessEntityID)}", businessEntityIdParameter);
        }
    }
#endif
}
