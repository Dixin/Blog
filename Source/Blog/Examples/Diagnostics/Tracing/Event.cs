namespace Examples.Diagnostics.Tracing
{
    using System.Diagnostics.Tracing;

    [EventSource(Name = nameof(Event), Guid = "ae6fa44a-c41a-495f-a618-c49ade52e682")]
    public class Event : EventSource
    {
        public const int BeginEventId = 1;

        public const int EndEventId = 2;

        public static Event Source { get; } = new Event();

        [Event(BeginEventId)]
        public void Begin(string arg) => this.WriteEvent(BeginEventId, arg);

        [Event(EndEventId)]
        public void End(string arg) => this.WriteEvent(EndEventId, arg);
    }
}
