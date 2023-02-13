namespace Fiero.Business
{
    public partial class MetaSystem
    {
        public readonly record struct EventRaisedEvent(string SystemName, string EventName, object Event);
    }
}
