namespace Fiero.Business
{
    public partial class ScriptingSystem
    {

        public readonly struct ScriptLoadedEvent
        {
            public readonly Script Script;

            public ScriptLoadedEvent(Script script)
            {
                Script = script;
            }
        }
    }
}
