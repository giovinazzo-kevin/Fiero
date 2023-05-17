namespace Fiero.Business
{
    public partial class ErgoScriptingSystem
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
