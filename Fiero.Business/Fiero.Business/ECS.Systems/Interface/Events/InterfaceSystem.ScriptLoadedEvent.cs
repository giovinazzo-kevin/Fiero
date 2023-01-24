namespace Fiero.Business
{
    public partial class InterfaceSystem
    {
        public readonly struct ScriptLoadedEvent
        {
            public readonly Script Script;

            public ScriptLoadedEvent(Script s) => Script = s;
        }
    }
}
