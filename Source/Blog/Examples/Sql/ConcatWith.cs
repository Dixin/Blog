namespace Examples.Sql;

using System.Data.SqlTypes;

using Microsoft.SqlServer.Server;

[Serializable]
[SqlUserDefinedAggregate(
    Format.UserDefined,
    IsInvariantToNulls = true,
    IsInvariantToDuplicates = false,
    IsInvariantToOrder = false,
    MaxByteSize = 8000)]
public class ConcatWith : IBinarySerialize
{
    private StringBuilder? concatWith;

    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
    public void Init() { }

    public void Accumulate(SqlString sqlString, SqlString separator) => this.concatWith = this.concatWith?
            .Append(separator.IsNull ? null : separator.Value)
            .Append(sqlString.IsNull ? null : sqlString.Value)
        ?? new(sqlString.IsNull ? null : sqlString.Value);

    public void Merge(ConcatWith concatWith) => this.concatWith?.Append(concatWith.concatWith);

    public SqlString Terminate() => new SqlString(this.concatWith?.ToString());

    public void Read(BinaryReader reader) => this.concatWith = new(reader.ReadString());

    public void Write(BinaryWriter writer) => writer.Write(this.concatWith?.ToString() ?? string.Empty);
}