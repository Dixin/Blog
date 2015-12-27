namespace EntityFramework.Functions.Tests.Examples
{
    using System.ComponentModel.DataAnnotations.Schema;

    [ComplexType]
    public class ContactInformation
    {
        public int PersonID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string JobTitle { get; set; }

        public string BusinessEntityType { get; set; }
    }

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
}
