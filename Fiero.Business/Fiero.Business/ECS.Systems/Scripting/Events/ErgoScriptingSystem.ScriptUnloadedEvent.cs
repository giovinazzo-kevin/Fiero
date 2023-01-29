namespace Fiero.Business
{
    public partial class ErgoScriptingSystem
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
