using Ergo.Interpreter;
using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Unconcern.Common;

namespace Fiero.Core
{
    public class ErgoScript : Script
    {
        public readonly ErgoVM VM;
        private readonly HashSet<EventHook> hooks = new();

        public readonly TermMarshallingContext MarshallingContext = new();
        public readonly Dictionary<EventHook, (Hook Hook, ErgoVM.Op Op)> ErgoHooks = new();

        private volatile bool running = false;


        public ErgoScript(InterpreterScope scope)
        {
            VM = scope.Facade.BuildVM(
                scope.BuildKnowledgeBase(CompilerFlags.Default),
                DecimalType.CliDecimal);
            hooks = scope.GetLibrary<CoreLib>(ErgoModules.Core)
                .GetScriptSubscriptions(this)
                .Select(s => new EventHook(s.Module.GetOrThrow().Explain().ToCSharpCase(), s.Functor.Explain().ToCSharpCase()))
                .ToHashSet();
        }

        public override IEnumerable<EventHook> Hooks => hooks;
        public override Subscription Run(ScriptRoutes routes)
        {
            if (running)
                throw new InvalidOperationException();
            running = true;
            var subs = new Subscription(new Action[] { () => running = false });
            foreach (var hook in hooks)
            {
                if (routes.TryGetValue(hook, out var route))
                    subs.Add(route(this));
                else
                {
                    // TODO: Check whether it's a script event or a missing route
                }
            }
            return subs;
        }
    }
}
