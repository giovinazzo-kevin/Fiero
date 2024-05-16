using Ergo.Interpreter;
using Ergo.Interpreter.Directives;
using Ergo.Interpreter.Libraries;
using Ergo.Lang.Ast;
using Ergo.Runtime.BuiltIns;
using Fiero.Core.Ergo.Libraries.Core.Data;
using Fiero.Core.Ergo.Libraries.Core.Event;
using Fiero.Core.Ergo.Libraries.Core.Input;

namespace Fiero.Core.Ergo.Libraries.Core
{
    [SingletonDependency]
    public class CoreLib(GameDataStore store, GameInput input, MetaSystem meta) : Library
    {
        public override Atom Module => CoreErgoModules.Core;
        protected readonly Dictionary<Atom, HashSet<Signature>> Subscribptions = new();
        protected readonly Dictionary<Atom, HashSet<Signature>> ObservedData = new();
        public void SubscribeToEvent(Atom scriptModule, Atom eventModule, Atom @event)
        {
            if (!Subscribptions.TryGetValue(scriptModule, out var set))
                set = Subscribptions[scriptModule] = new();
            set.Add(new(@event, 1, eventModule, default));
        }
        public void ObserveDatum(Atom scriptModule, Atom dataModule, Atom datumName)
        {
            if (!ObservedData.TryGetValue(scriptModule, out var set))
                set = ObservedData[scriptModule] = new();
            set.Add(new(datumName, 2, dataModule, default));
        }
        public IEnumerable<Signature> GetScriptSubscriptions(InterpreterScope scope)
        {
            var set = new HashSet<Signature>();
            var modules = scope.VisibleModules;
            foreach (var m in modules)
            {
                if (Subscribptions.TryGetValue(m, out var inner))
                    set.UnionWith(inner);
            }
            return set;
        }
        public IEnumerable<Signature> GetObservedData(InterpreterScope scope)
        {
            var set = new HashSet<Signature>();
            var modules = scope.VisibleModules;
            foreach (var m in modules)
            {
                if (ObservedData.TryGetValue(m, out var inner))
                    set.UnionWith(inner);
            }
            return set;
        }
        public override IEnumerable<BuiltIn> GetExportedBuiltins()
        {
            yield return new GetDatum(store);
            yield return new SetDatum(store);
            yield return new KeyState(input);
            yield return new SimulateKey(input);
            yield return new Raise(meta);
        }
        public override IEnumerable<InterpreterDirective> GetExportedDirectives()
        {
            yield return new SubscribeToEvent();
            yield return new ObserveDatum();
        }
    }
}
