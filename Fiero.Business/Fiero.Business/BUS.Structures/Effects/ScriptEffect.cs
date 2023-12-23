using Ergo.Interpreter;
using Ergo.Interpreter.Libraries;
using Ergo.Lang;
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

        private ScriptEffectLib Lib => Script.ScriptProperties.KnowledgeBase.Scope.GetLibrary<ScriptEffectLib>();

        protected ErgoVM GetOrCreateContext(GameSystems systems, Entity owner)
        {
            if (Contexts.TryGetValue(owner.Id, out var ctx))
                return ctx;
            var newScope = Script.ScriptProperties.KnowledgeBase.Scope;
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
            Lib.SetCurrentOwner(this, owner, ArgumentsString);
            ctx.Query = EffectStartedHook;
            ctx.Run();
        }

        protected override void OnEnded(GameSystems systems, Entity owner)
        {
            base.OnEnded(systems, owner);
            var ctx = GetOrCreateContext(systems, owner);
            Lib.SetCurrentOwner(this, owner, ArgumentsString);
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
