using Ergo.Interpreter;
using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Unconcern.Common;

namespace Fiero.Core
{
    public class ErgoScript : Script
    {
        public readonly ErgoVM VM;
        private readonly HashSet<EventHook> eventHooks = new();
        private readonly HashSet<DataHook> dataHooks = new();

        internal readonly TermMarshallingContext MarshallingContext = new();
        internal readonly Dictionary<EventHook, (Hook Hook, ErgoVM.Op Op)> ErgoEventHooks = new();
        internal readonly Dictionary<DataHook, (Hook Hook, ErgoVM.Op Op)> ErgoDataHooks = new();

        private volatile bool running = false;

        private readonly Atom subscribed = new("subscribed");

        public ErgoScript(InterpreterScope scope)
        {
            VM = scope.Facade.BuildVM(
                scope.BuildKnowledgeBase(CompilerFlags.Default),
                DecimalType.CliDecimal);
            var coreLib = scope.GetLibrary<CoreLib>(ErgoModules.Core);
            eventHooks = coreLib
                .GetScriptSubscriptions(this)
                .Select(s => new EventHook(s.Module.GetOrThrow().Explain().ToCSharpCase(), s.Functor.Explain().ToCSharpCase()))
                .ToHashSet();
            dataHooks = coreLib
                .GetObservedData(this)
                .Select(s => new DataHook(s.ToCSharpCase()))
                .ToHashSet();
            foreach (var sub in coreLib
                .GetScriptSubscriptions(this))
            {
                var fact = Predicate.Fact(ErgoModules.Event, new Complex(subscribed, sub.Module.GetOrThrow(), sub.Functor), dynamic: true, exported: true);
                VM.KB.AssertZ(fact);
            }
        }
        public override Subscription Run(ScriptEventRoutes eventRoutes, ScriptDataRoutes dataRoutes)
        {
            if (running)
                throw new InvalidOperationException();
            running = true;
            var subs = new Subscription(new Action[] { () => running = false });
            foreach (var hook in eventHooks)
            {
                if (eventRoutes.TryGetValue(hook, out var route))
                    subs.Add(route(this));
                else
                {
                    // TODO: Check whether it's a script event or a missing route
                }
            }
            foreach (var hook in dataHooks)
            {
                if (dataRoutes.TryGetValue(hook, out var route))
                    subs.Add(route(this));
                else
                {
                    // TODO: Check whether it's a script event or a missing route
                }
            }
            return subs;
        }
        public override IEnumerable<EventHook> EventHooks => eventHooks;
        public override IEnumerable<DataHook> DataHooks => dataHooks;
    }
}
