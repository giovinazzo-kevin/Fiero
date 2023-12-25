using Ergo.Interpreter.Directives;
using Ergo.Interpreter.Libraries;
using Ergo.Lang.Ast;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Core
{
    [SingletonDependency]
    public class CoreLib(GameDataStore store, GameInput input, MetaSystem meta) : Library
    {
        public override Atom Module => ErgoModules.Core;
        protected readonly Dictionary<Atom, HashSet<Signature>> Subscribptions = new();
        protected readonly Dictionary<Atom, HashSet<string>> ObservedData = new();
        public void SubscribeToEvent(Atom scriptModule, Atom eventModule, Atom @event)
        {
            if (!Subscribptions.TryGetValue(scriptModule, out var set))
                set = Subscribptions[scriptModule] = new();
            set.Add(new(@event, 1, eventModule, default));
        }
        public void ObserveDatum(Atom scriptModule, string name)
        {
            if (!ObservedData.TryGetValue(scriptModule, out var set))
                set = ObservedData[scriptModule] = new();
            set.Add(name);
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
        public IEnumerable<string> GetObservedData(ErgoScript script)
        {
            var set = new HashSet<string>();
            var modules = script.VM.KB.Scope.VisibleModules;
            foreach (var m in modules)
            {
                if (ObservedData.TryGetValue(m, out var inner))
                    set.UnionWith(inner);
            }
            return set;
        }
        public override IEnumerable<BuiltIn> GetExportedBuiltins()
        {
            yield return new Get(store);
            yield return new Set(store);
            yield return new KeyState(input);
            yield return new Raise(meta);
        }
        public override IEnumerable<InterpreterDirective> GetExportedDirectives()
        {
            yield return new SubscribeToEvent();
            yield return new ObserveDatum();
        }
    }
}
