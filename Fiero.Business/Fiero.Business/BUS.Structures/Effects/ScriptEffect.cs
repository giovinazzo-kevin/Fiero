using Ergo.Interpreter;
using Ergo.Interpreter.Directives;
using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using System.Collections.Immutable;
using Unconcern.Common;

namespace Fiero.Business
{
    internal class ScriptEffectLib : Library
    {
        internal class End(GameSystems systems, ScriptEffect source, Entity owner, Atom module)
            : BuiltIn(string.Empty, new Atom("end"), 0, module)
        {
            protected readonly ScriptEffect _source = source;
            protected readonly Entity _owner = owner;
            protected readonly GameSystems _systems = systems;
            private bool _ended = false;

            public override ErgoVM.Op Compile()
            {
                return vm =>
                {
                    if (_ended)
                        vm.Fail();
                    else
                    {
                        _source.End(_systems, _owner);
                        _ended = true;
                    }
                };
            }
        }
        internal class Owner(Entity owner, Atom module)
            : BuiltIn(string.Empty, new Atom("owner_"), 1, module)
        {
            protected readonly EntityAsTerm _owner = new(owner.Id, owner.ErgoType());

            public override ErgoVM.Op Compile()
            {
                return vm =>
                {
                    vm.SetArg(1, _owner);
                    ErgoVM.Goals.Unify2(vm);
                };
            }
        }
        internal class Args(string args, Atom module)
            : BuiltIn(string.Empty, new Atom("args"), 1, module)
        {
            protected readonly string _args = args;

            public override ErgoVM.Op Compile()
            {
                return vm =>
                {
                    if (!vm.KB.Scope.Parse<ITerm>(_args).TryGetValue(out var term))
                    {
                        vm.Fail();
                        return;
                    }
                    vm.SetArg(1, term);
                    ErgoVM.Goals.Unify2(vm);
                };
            }
        }
        internal class Subscribed(List<Signature> subs, Atom module) : BuiltIn(string.Empty, new Atom("subscribed"), 2, module)
        {
            private readonly List<Signature> _subs = subs;

            public override ErgoVM.Op Compile()
            {
                int i = 0;
                return NextSub;
                void NextSub(ErgoVM vm)
                {
                    var sign = _subs[i++];
                    if (i < _subs.Count)
                        vm.PushChoice(NextSub);
                    else i = 0;
                    var module = sign.Module.GetOr(default);
                    var a1 = vm.Arg(1);
                    vm.SetArg(1, module);
                    ErgoVM.Goals.Unify2(vm);
                    if (vm.State == ErgoVM.VMState.Fail)
                        return;
                    vm.SetArg(0, a1);
                    vm.SetArg(1, sign.Functor);
                    ErgoVM.Goals.Unify2(vm);
                }
            }
        }


        private readonly Atom _module;
        public override Atom Module => _module;
        protected readonly ScriptEffect _source;
        protected readonly GameSystems _systems;
        protected readonly Entity _owner;
        protected readonly string _args;
        public ScriptEffectLib(GameSystems systems, ScriptEffect source, Entity owner, string arguments, Atom module)
        {
            _module = module;
            _source = source;
            _owner = owner;
            _systems = systems;
            _args = arguments;
        }
        public override IEnumerable<BuiltIn> GetExportedBuiltins()
        {
            yield return new End(_systems, _source, _owner, _module);
            yield return new Args(_args, _module);
            yield return new Owner(_owner, _module);
            yield return new Subscribed(_source.Script.ScriptProperties.SubscribedEvents, _module);
        }
        public override IEnumerable<InterpreterDirective> GetExportedDirectives()
        {
            yield break;
        }
    }

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
        public ErgoVM.Op EffectStartedHook { get; private set; }
        public ErgoVM.Op EffectEndedHook { get; private set; }
        public ErgoVM.Op ClearDataHook { get; private set; }
        public readonly string ArgumentsString;

        public readonly Dictionary<int, ErgoVM> Contexts = new();

        public ScriptEffect(Script script, string arguments, string description = null)
        {
            Script = script;
            ArgumentsString = arguments;
            Description = description ?? string.Empty;
        }

        public override EffectName Name => EffectName.Script;
        public override string DisplayName => Script.Info.Name;
        public override string DisplayDescription => Description;

        protected ErgoVM GetOrCreateContext(GameSystems systems, Entity owner)
        {
            if (Contexts.TryGetValue(owner.Id, out var ctx))
                return ctx;
            var newScope = Script.ScriptProperties.KnowledgeBase.Scope;
            var lib = new ScriptEffectLib(systems, this, owner, ArgumentsString, newScope.Entry);
            // Since the kb is already created by now, we need to assert these builtins manually
            foreach (var b in lib.GetExportedBuiltins())
            {
                var pred = new Predicate(b);
                Script.ScriptProperties.KnowledgeBase.AssertZ(pred);
                Script.ScriptProperties.KnowledgeBase.DependencyGraph.AddNode(pred);
            }
            var module = newScope.Entry;
            EffectStartedHook = new Hook(new(new("began"), Maybe.Some(0), module, default))
                .Compile(throwIfNotDefined: true);
            EffectEndedHook = new Hook(new(new("ended"), Maybe.Some(0), module, default))
                .Compile(throwIfNotDefined: true);
            ClearDataHook = new Hook(new(new("clear"), Maybe.Some(0), ScriptingSystem.DataModule, default))
                .Compile(throwIfNotDefined: false);
            return Contexts[owner.Id] = newScope.Facade
                .SetInput(systems.Scripting.InReader, newScope.Facade.InputReader)
                .SetOutput(systems.Scripting.OutWriter)
                .BuildVM(Script.ScriptProperties.KnowledgeBase, DecimalType.CliDecimal);
        }

        protected override void OnStarted(GameSystems systems, Entity owner)
        {
            base.OnStarted(systems, owner);
            if (Script.ScriptProperties.LastError != null)
                return;
            var ctx = GetOrCreateContext(systems, owner);
            ctx.Query = EffectStartedHook;
            ctx.Run();
        }

        protected override void OnEnded(GameSystems systems, Entity owner)
        {
            base.OnEnded(systems, owner);
            var ctx = GetOrCreateContext(systems, owner);
            ctx.Query = EffectEndedHook;
            ctx.Run();
            ctx.Query = ClearDataHook;
            ctx.Run();
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
