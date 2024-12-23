namespace MediaInfoLib;

public partial class MediaInfo : IDisposable
{
    public void Dispose()
    {
        this.Close();
        // GC.SuppressFinalize(this);
    }
}