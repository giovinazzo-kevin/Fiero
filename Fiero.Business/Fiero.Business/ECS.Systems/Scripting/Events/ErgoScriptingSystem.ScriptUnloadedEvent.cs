namespace Fiero.Business
{
    public partial class ScriptingSystem
    {
        public readonly struct ScriptUnloadedEvent
        {
            public readonly Script Script;

            public ScriptUnloadedEvent(Script script)
            {
                Script = script;
            }
        }
    }
}
