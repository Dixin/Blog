namespace EntityFramework.Functions
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;

    public enum FunctionType
    {
        StoredProcedure = 0,

        TableValuedFunction,

        ComposableScalarValuedFunction,

        NonComposableScalarValuedFunction,

        AggregateFunction,

        BuiltInFunction,

        NiladicFunction
    }

    // <Function Name="uspGetManagerEmployees" Aggregate="false" BuiltIn="false" NiladicFunction="false" IsComposable="false" ParameterTypeSemantics="AllowImplicitConversion" Schema="dbo">
    [AttributeUsage(AttributeTargets.Method)]
    public class FunctionAttribute : DbFunctionAttribute
    {
        public FunctionAttribute(string name, FunctionType functionType)
            : base(Function.CodeFirstDatabaseSchema, name) // DbFunctionAttribute has FunctionName property.
        {
            this.Type = functionType;
            switch (functionType)
            {
                case FunctionType.StoredProcedure:
                case FunctionType.NonComposableScalarValuedFunction:
                    this.IsComposable = false;
                    this.IsAggregate = false;
                    this.IsBuiltIn = false;
                    this.IsNiladic = false;
                    break;

                case FunctionType.TableValuedFunction:
                case FunctionType.ComposableScalarValuedFunction:
                    this.IsComposable = true;
                    this.IsAggregate = false;
                    this.IsBuiltIn = false;
                    this.IsNiladic = false;
                    break;

                case FunctionType.AggregateFunction:
                    this.IsComposable = true;
                    this.IsAggregate = true;
                    this.IsBuiltIn = false;
                    this.IsNiladic = false;
                    break;

                case FunctionType.BuiltInFunction:
                    this.IsComposable = true;
                    this.IsAggregate = false;
                    this.IsBuiltIn = true;
                    this.IsNiladic = false;
                    break;

                case FunctionType.NiladicFunction:
                    this.IsComposable = true;
                    this.IsAggregate = false;
                    this.IsBuiltIn = true;
                    this.IsNiladic = true;
                    break;
            }
        }

        public FunctionType Type { get; }

        public bool IsComposable { get; }

        public bool IsAggregate { get; }

        public bool IsBuiltIn { get; }

        public bool IsNiladic { get; }

        public string Schema { get; set; }

        public ParameterTypeSemantics ParameterTypeSemantics { get; set; } = ParameterTypeSemantics.AllowImplicitConversion;
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