namespace Examples.IO;

using Examples.Common;

public class CustomTextWriter : TextWriter
{
    private readonly Action<string?> write;

    public CustomTextWriter(Action<string?> write, Encoding? encoding = null)
    {
        write.NotNull(nameof(write));

        this.write = write;
        this.Encoding = encoding ?? Encoding.Default;
    }

    public override void Write(string? value) => this.write(value);

    public override void Write(char[] buffer, int index, int count) => this.Write(new string(buffer, index, count));

    public override Encoding Encoding { get; }
}