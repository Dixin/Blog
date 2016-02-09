namespace Dixin.Sql
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

        private StringBuilder concat;

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void Init()
        {
        }

        public void Accumulate(SqlString sqlString) => this.concat = this.concat?
            .Append(Separator).Append(sqlString.IsNull ? null : sqlString.Value)
            ?? new StringBuilder(sqlString.IsNull ? null : sqlString.Value);

        public void Merge(Concat concat) => this.concat.Append(concat.concat);

        public SqlString Terminate() => new SqlString(this.concat?.ToString());

        public void Read(BinaryReader reader) => this.concat = new StringBuilder(reader.ReadString());

        public void Write(BinaryWriter writer) => writer.Write(this.concat?.ToString() ?? string.Empty);
    }
}
