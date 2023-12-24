using Ergo.Interpreter.Directives;
using Ergo.Interpreter.Libraries;
using Ergo.Lang.Ast;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Core
{
    public class CoreLib : Library
    {
        public override Atom Module => ErgoModules.Core;
        protected readonly Dictionary<Atom, HashSet<Signature>> Subscribptions = new();
        public void SubscribeScriptToEvent(Atom scriptModule, Atom eventModule, Atom @event)
        {
            if (!Subscribptions.TryGetValue(scriptModule, out var set))
                set = Subscribptions[scriptModule] = new();
            set.Add(new(@event, 1, eventModule, default));
        }
        public IEnumerable<Signature> GetScriptSubscriptions(ErgoScript script)
        {
            var set = new HashSet<Signature>();
            var modules = script.VM.KB.Scope.VisibleModules;
            foreach (var m in modules)
            {
                if (Subscribptions.TryGetValue(m, out var inner))
                    set.UnionWith(inner);
            }
            return set;
        }
        public override IEnumerable<BuiltIn> GetExportedBuiltins()
        {
            yield break;
        }
        public override IEnumerable<InterpreterDirective> GetExportedDirectives()
        {
            yield return new SubscribeToEvent();
        }
    }
}
