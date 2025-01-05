namespace MediaInfoLib;

public partial class MediaInfo : IDisposable
{
    public void Dispose()
    {
        this.Close();
        // GC.SuppressFinalize(this);
    }

    public MediaInfo(string media) : this()
    {
        this.Open(media);
        this.Option("Complete"); //or Option("Complete", "1") or Option("Info_Parameters")
    }

    public IEnumerable<string> Enumerate(StreamKind streamKind, string parameter) =>
        Enumerable
            .Range(0, this.Count_Get(streamKind))
            .Select(index => this.Get(streamKind, index, parameter));

    public string[] Get(StreamKind streamKind, string parameter) => 
        this.Enumerate(streamKind, parameter).ToArray();

    public bool Any(StreamKind streamKind, string parameter, Func<string, bool>? predicate = null) => 
        this.Enumerate(streamKind, parameter).Any(value => predicate?.Invoke(value) ?? true);
}