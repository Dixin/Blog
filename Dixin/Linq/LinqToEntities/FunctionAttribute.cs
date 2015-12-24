namespace Dixin.Linq.LinqToEntities
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;

    // <Function Name="uspGetManagerEmployees" Aggregate="false" BuiltIn="false" NiladicFunction="false" IsComposable="false" ParameterTypeSemantics="AllowImplicitConversion" Schema="dbo">
    [AttributeUsage(AttributeTargets.Method)]
    public class FunctionAttribute : DbFunctionAttribute
    {
        public FunctionAttribute(string name)
            : base(FunctionConvention.CodeFirstDatabaseSchema, name)
        {
        }

        public FunctionAttribute(
            string name,
            bool isComposable,
            bool isAggregate = false,
            bool isBuiltIn = false,
            bool isNiladic = false,
            ParameterTypeSemantics parameterTypeSemantics = System.Data.Entity.Core.Metadata.Edm.ParameterTypeSemantics.AllowImplicitConversion)
            : base(FunctionConvention.CodeFirstDatabaseSchema, name)
        {
            this.IsComposable = isComposable;
            this.IsAggregate = isAggregate;
            this.IsBuiltIn = isBuiltIn;
            this.IsNiladic = isNiladic;
            this.ParameterTypeSemantics = parameterTypeSemantics;
        }

        // FunctionName property is inherited from DbFunctionAttribute.

        public string Schema { get; set; }

        public bool? IsComposable { get; }

        public bool? IsAggregate { get; set; }

        public bool? IsBuiltIn { get; set; }

        public bool? IsNiladic { get; set; }

        public ParameterTypeSemantics? ParameterTypeSemantics { get; set; }
    }

    // System.Data.Linq.Mapping.ParameterAttribute
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class ParameterAttribute : Attribute
    {
        public string Name { get; set; }

        public string DbType { get; set; }

        public Type ClrType { get; set; }
    }

    // System.Data.Linq.Mapping.ResultTypeAttribute
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ResultTypeAttribute : Attribute
    {
        public ResultTypeAttribute(Type type)
        {
            this.Type = type;
        }

        public Type Type { get; }
    }
}