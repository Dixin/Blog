namespace MediaManager;

public interface IDefaultSettings
{
    static abstract ISettings Settings { get; set; }
}