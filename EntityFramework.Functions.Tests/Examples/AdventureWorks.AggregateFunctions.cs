namespace EntityFramework.Functions.Tests.Examples
{
    using System.Collections.Generic;

    public static class AdventureWorksFunctions
    {
        // Defines aggregate function, which must have one singele IEnumerable<T> or IQueryable<T> parameter.
        // It can only be used in LINQ to Entities queries, where its body will never be executed;
        // and cannot be called directly.
        [Function(FunctionType.AggregateFunction, nameof(Concat), Schema = AdventureWorksDbContext.dbo)]
        public static string Concat(this IEnumerable<string> value) => Function.CallNotSupported<string>();

        // Aggregate function with more than more parameter is not supported by Entity Framework.
        // The following cannot to translated in LINQ queries.
        // [Function(FunctionType.AggregateFunction, nameof(ConcatWith), Schema = AdventureWorks.dbo)]
        // public static string ConcatWith(this IEnumerable<string> value, string separator) => 
        //    Function.CallNotSupported<string>();

        // The Concat and ConcatWith aggregate functions are implemented as below:
    }

    // using System;
    // using System.Data.SqlTypes;
    // using System.IO;
    // using System.Text;
    //
    // using Microsoft.SqlServer.Server;
    //
    // [Serializable]
    // [SqlUserDefinedAggregate(
    //    Format.UserDefined,
    //    IsInvariantToNulls = true,
    //    IsInvariantToDuplicates = false,
    //    IsInvariantToOrder = false,
    //    MaxByteSize = 8000)]
    // public class Concat : IBinarySerialize
    // {
    //    private const string Separator = ", ";
    //
    //    private StringBuilder concat;
    //
    //    public void Init()
    //    {
    //    }
    //
    //    public void Accumulate(SqlString sqlString) => this.concat = this.concat?
    //        .Append(Separator).Append(sqlString.IsNull ? null : sqlString.Value)
    //        ?? new StringBuilder(sqlString.IsNull ? null : sqlString.Value);
    //
    //    public void Merge(Concat concat) => this.concat.Append(concat.concat);
    //
    //    public SqlString Terminate() => new SqlString(this.concat?.ToString());
    //
    //    public void Read(BinaryReader reader) => this.concat = new StringBuilder(reader.ReadString());
    //
    //    public void Write(BinaryWriter writer) => writer.Write(this.concat?.ToString() ?? string.Empty);
    // }

    // [Serializable]
    // [SqlUserDefinedAggregate(
    // Format.UserDefined,
    // IsInvariantToNulls = true,
    // IsInvariantToDuplicates = false,
    // IsInvariantToOrder = false,
    // MaxByteSize = 8000)]
    // public class ConcatWith : IBinarySerialize
    // {
    //    private StringBuilder concatWith;
    //
    //    public void Init()
    //    {
    //    }
    //
    //    public void Accumulate(SqlString sqlString, SqlString separator) => this.concatWith = this.concatWith?
    //        .Append(separator.IsNull ? null : separator.Value)
    //        .Append(sqlString.IsNull ? null : sqlString.Value)
    //        ?? new StringBuilder(sqlString.IsNull ? null : sqlString.Value);
    //
    //    public void Merge(ConcatWith concatWith) => this.concatWith.Append(concatWith.concatWith);
    //
    //    public SqlString Terminate() => new SqlString(this.concatWith?.ToString());
    //
    //    public void Read(BinaryReader reader) => this.concatWith = new StringBuilder(reader.ReadString());
    //
    //    public void Write(BinaryWriter writer) => writer.Write(this.concatWith?.ToString() ?? string.Empty);
    // }
}
