using Ergo.Lang.Ast;

namespace Fiero.Business
{
    public partial class ScriptingSystem
    {
        public readonly record struct ScriptEventRaisedEvent(string System, string Event, ITerm Data);
    }
}
