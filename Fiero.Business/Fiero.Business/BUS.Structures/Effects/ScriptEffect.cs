using Ergo.Interpreter;
using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Compiler;
using Ergo.Runtime;
using System.Collections.Immutable;
using Unconcern.Common;

namespace Fiero.Business
{

    /// <summary>
    /// Script effects can be applied to:
    /// - Entities:
    ///     - The effect is applied to the entity when the effect starts, and is removed when the effect ends.
    /// </summary>
    public class ScriptEffect : Effect
    {
        public readonly Script Script;
        public readonly InterpreterScope Scope;
        public readonly string Description;
        public readonly ErgoVM.Op EffectStartedHook;
        public readonly ErgoVM.Op EffectEndedHook;
        public readonly ErgoVM.Op ClearDataHook;
        public readonly string ArgumentsString;

        public readonly Dictionary<int, ErgoVM> Contexts = new();

        public ScriptEffect(Script script, string arguments, string description = null)
        {
            Script = script;
            ArgumentsString = arguments;
            Description = description ?? string.Empty;
            var module = script.ScriptProperties.KnowledgeBase.Scope.Entry;
            EffectStartedHook = new Hook(new(new("began"), Maybe.Some(0), module, default))
                .Compile(throwIfNotDefined: true);
            EffectEndedHook = new Hook(new(new("ended"), Maybe.Some(0), module, default))
                .Compile(throwIfNotDefined: true);
            ClearDataHook = new Hook(new(new("clear"), Maybe.Some(0), ScriptingSystem.DataModule, default))
                .Compile(throwIfNotDefined: false);
        }

        public override EffectName Name => EffectName.Script;
        public override string DisplayName => Script.Info.Name;
        public override string DisplayDescription => Description;

        protected ErgoVM GetOrCreateContext(GameSystems systems, Entity owner)
        {
            if (Contexts.TryGetValue(owner.Id, out var ctx))
                return ctx;
            var newScope = Script.ScriptProperties.KnowledgeBase.Scope;
            var newKb = Script.ScriptProperties.KnowledgeBase.Clone();
            Assert("owner", new EntityAsTerm(owner.Id, owner.ErgoType()));
            Assert("end__", new Atom((ErgoVM.Op)((ErgoVM _) => { End(systems, owner); })));
            if (!string.IsNullOrEmpty(ArgumentsString))
            {
                if (newKb.Scope.Parse<ITerm>(ArgumentsString).TryGetValue(out var args))
                    Assert("args", args);
                else
                    Assert("args", new Atom(ArgumentsString));
            }
            var invOp = new InvalidOperationException();
            foreach (var e in Script.ScriptProperties.SubscribedEvents)
            {
                Assert("subscribed", e.Module.GetOrThrow(invOp), e.Functor);
            }
            newKb.DependencyGraph.Rebuild();
            return Contexts[owner.Id] = newScope.Facade
                .SetInput(systems.Scripting.InReader, newScope.Facade.InputReader)
                .SetOutput(systems.Scripting.OutWriter)
                .BuildVM(newKb, DecimalType.CliDecimal);

            void Assert(string functor, params ITerm[] args)
            {
                var p = new Predicate("", ScriptingSystem.ScriptModule, new Complex(new(functor), args), NTuple.Empty, dynamic: true, exported: false, graph: default);
                p = p
                    .WithExecutionGraph(p.ToExecutionGraph(newKb.DependencyGraph));
                newKb.AssertZ(p);
            }
        }

        protected override void OnStarted(GameSystems systems, Entity owner)
        {
            base.OnStarted(systems, owner);
            if (Script.ScriptProperties.LastError != null)
                return;
            var ctx = GetOrCreateContext(systems, owner);
            ctx = ctx.ScopedInstance();
            ctx.Query = EffectStartedHook;
            ctx.Run();
        }

        protected override void OnEnded(GameSystems systems, Entity owner)
        {
            base.OnEnded(systems, owner);
            var ctx = GetOrCreateContext(systems, owner);
            ctx = ctx.ScopedInstance();
            ctx.Query = EffectEndedHook;
            ctx.Run();
            //ctx.Query = ClearDataHook;
            //ctx.Run();
            Contexts.Remove(owner.Id, out _);
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            _ = GetOrCreateContext(systems, owner);
            /* Ergo scripts can subscribe to Fiero events via the subscribe/2 directive.
               All the directive does is prepare a list for this method, which is
               called whenever an effect that is tied to a script resolves. The list
               contains the signatures of the events that the script is handling.

               By wiring each event to a call to the script's solver, we can interpret
               the result of that call as the EventResult to pass to the owning system.

                Additionally, Ergo scripts may raise and handle events of their own.
            */
            var routes = ScriptingSystem.GetScriptRoutes();
            var visibleScripts = systems.Scripting.GetVisibleScripts();
            var subbedEvents = Script.ScriptProperties.SubscribedEvents;
            foreach (var sig in subbedEvents)
            {
                if (routes.TryGetValue(sig, out var sub))
                {
                    yield return sub(this, systems);
                }
                // Could be from a script, in which case it's not unknown so much as it's dynamic
                else if (!visibleScripts.Contains(sig.Module.GetOrThrow(new InvalidOperationException()).Explain()))
                {
                    //Script.ScriptProperties.VM.Out
                    //    .WriteLine($"WRN: Unknown event route: {sig.Explain()}");
                    //Script.ScriptProperties.VM.Out.Flush();
                }
            }
        }
    }
}
