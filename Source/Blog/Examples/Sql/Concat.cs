namespace Examples.Sql
{
    using System;
    using System.Data.SqlTypes;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;

    using Microsoft.SqlServer.Server;

    [Serializable]
    [SqlUserDefinedAggregate(
        Format.UserDefined,
        IsInvariantToNulls = true,
        IsInvariantToDuplicates = false,
        IsInvariantToOrder = false,
        MaxByteSize = 8000)]
    public class Concat : IBinarySerialize
    {
        private const string Separator = ", ";

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        private StringBuilder concat;
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void Init()
        {
        }

        public void Accumulate(SqlString sqlString) => this.concat = this.concat?
            .Append(Separator).Append(sqlString.IsNull ? null : sqlString.Value)
            ?? new(sqlString.IsNull ? null : sqlString.Value);

        public void Merge(Concat concat) => this.concat.Append(concat.concat);

        public SqlString Terminate() => new(this.concat?.ToString());

        public void Read(BinaryReader reader) => this.concat = new(reader.ReadString());

        public void Write(BinaryWriter writer) => writer.Write(this.concat?.ToString() ?? string.Empty);
    }
}
