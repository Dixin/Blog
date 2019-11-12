namespace Tutorial.LinqToSql
{
    using System;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    public partial class ManagerEmployee
    {
        [Column(DbType = "Int")]
        public int? RecursionLevel { get; set; }

        [Column(DbType = "NVarChar(4000)")]
        public string OrganizationNode { get; set; }

        [Column(DbType = "NVarChar(50) NOT NULL", CanBeNull = false)]
        public string ManagerFirstName { get; set; }

        [Column(DbType = "NVarChar(50) NOT NULL", CanBeNull = false)]
        public string ManagerLastName { get; set; }

        [Column(DbType = "Int")]
        public int? BusinessEntityID { get; set; }

        [Column(DbType = "NVarChar(50)")]
        public string FirstName { get; set; }

        [Column(DbType = "NVarChar(50)")]
        public string LastName { get; set; }
    }

    public partial class ContactInformation
    {
        [Column(DbType = "int NOT NULL")]
        public int PersonID { get; set; }

        [Column(DbType = "nvarchar(50)")]
        public string FirstName { get; set; }

        [Column(DbType = "nvarchar(50)")]
        public string LastName { get; set; }

        [Column(DbType = "nvarchar(50)")]
        public string JobTitle { get; set; }

        [Column(DbType = "nvarchar(50)")]
        public string BusinessEntityType { get; set; }
    }

    public partial class AdventureWorks
    {
        [Function(Name = "dbo.uspGetManagerEmployees")]
        public ISingleResult<ManagerEmployee> uspGetManagerEmployees(
            [Parameter(Name = "BusinessEntityID", DbType = "int")] int? businessEntityID)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), businessEntityID);
            return (ISingleResult<ManagerEmployee>)result.ReturnValue;
        }

        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        [Function(Name = "dbo.uspLogError")]
        public int uspLogError(
            [Parameter(Name = "ErrorLogID", DbType = "int")] ref int? errorLogID)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), errorLogID);
            errorLogID = (int?)result.GetParameterValue(0);
            return (int)result.ReturnValue;
        }

        [Function(Name = "dbo.uspGetCategoryAndSubcategory")]
        [ResultType(typeof(ProductCategory))]
        [ResultType(typeof(ProductSubcategory))]
        public IMultipleResults uspGetCategoryAndSubcategory(
            [Parameter(Name = "CategoryID", DbType = "int")] int? categoryID)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), categoryID);
            return (IMultipleResults)result.ReturnValue;
        }

        [Function(Name = "dbo.ufnGetContactInformation", IsComposable = true)]
        public IQueryable<ContactInformation> ufnGetContactInformation(
            [Parameter(Name = "PersonID", DbType = "int")] int? personID) =>
                this.CreateMethodCallQuery<ContactInformation>(
                    this, (MethodInfo)MethodBase.GetCurrentMethod(), personID);

        [Function(Name = "dbo.ufnGetProductListPrice", IsComposable = true)]
        public decimal? ufnGetProductListPrice(
            [Parameter(Name = "ProductID", DbType = "int")] int? productID,
            [Parameter(Name = "OrderDate", DbType = "datetime")] DateTime? orderDate) =>
                (decimal?)this.ExecuteMethodCall(
                    this, (MethodInfo)MethodBase.GetCurrentMethod(), productID, orderDate).ReturnValue;

        [Function(Name = "dbo.ufnGetProductStandardCost", IsComposable = true)]
        public decimal? ufnGetProductStandardCost(
            [Parameter(Name = "ProductID", DbType = "int")] int? productID,
            [Parameter(Name = "OrderDate", DbType = "datetime")] DateTime? orderDate) =>
                (decimal?)this.ExecuteMethodCall(
                    this, (MethodInfo)MethodBase.GetCurrentMethod(), productID, orderDate).ReturnValue;
    }
}