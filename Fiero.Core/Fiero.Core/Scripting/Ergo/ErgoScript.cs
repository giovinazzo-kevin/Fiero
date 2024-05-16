using Ergo.Interpreter;
using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Fiero.Core.Ergo.Libraries.Core;
using Unconcern.Common;

namespace Fiero.Core.Ergo
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
        private readonly Atom observed = new("observed");

        public ErgoScript(InterpreterScope scope)
            : base(scope.Entry.Explain())
        {
            /*
             Scripts may subscribe to system events through the subscribe/2 directive.
             They may also subscribe to data change events through observe/2.

             In the case of script-raised events and data changes on datums that are not known at compile time,
             the predicates subscribed/2 and observed/3 are called by a global event handler (that looks the same for all scripts that import the data or event modules).
             This handler then builds the correct predicate at runtime and calls it.
             */
            var coreLib = scope.GetLibrary<CoreLib>(ErgoModules.Core);
            var facts = new List<Predicate>(GetFacts());
            VM = scope.Facade.BuildVM(
                scope.BuildKnowledgeBase(CompilerFlags.Default, beforeCompile: kb =>
                {
                    foreach (var fact in facts)
                        kb.AssertZ(fact);
                }),
                DecimalType.CliDecimal);
            eventHooks = coreLib
                .GetScriptSubscriptions(scope)
                .Select(s => new EventHook(s.Module.GetOrThrow().Explain().ToCSharpCase(), s.Functor.Explain().ToCSharpCase()))
                .ToHashSet();
            dataHooks = coreLib
                .GetObservedData(scope)
                .Select(s => new DataHook(s.Module.GetOrThrow().Explain().ToCSharpCase(), s.Functor.Explain().ToCSharpCase()))
                .ToHashSet();

            IEnumerable<Predicate> GetFacts()
            {
                var any = false;
                foreach (var sub in coreLib.GetScriptSubscriptions(scope))
                {
                    yield return Predicate.Fact(ErgoModules.Event, new Complex(subscribed, sub.Module.GetOrThrow(), sub.Functor), dynamic: false, exported: true);
                    any = true;
                }
                if (!any)
                    yield return Predicate.Falsehood(ErgoModules.Event, new Complex(subscribed, WellKnown.Literals.Discard, WellKnown.Literals.Discard), dynamic: false, exported: true);
                any = false;
                foreach (var sub in coreLib.GetObservedData(scope))
                {
                    yield return Predicate.Fact(ErgoModules.Data, new Complex(observed, sub.Module.GetOrThrow(), sub.Functor, new Atom((string)sub.Functor.Value + "_changed")));
                    any = true;
                }
                if (!any)
                    yield return Predicate.Falsehood(ErgoModules.Data, new Complex(observed, WellKnown.Literals.Discard, WellKnown.Literals.Discard, WellKnown.Literals.Discard));
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
                    subs.Add([route(this)]);
                else
                {
                    // TODO: Check whether it's a script event or a missing route
                }
            }
            foreach (var hook in dataHooks)
            {
                if (dataRoutes.TryGetValue(hook, out var route))
                    subs.Add([route(this)]);
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
