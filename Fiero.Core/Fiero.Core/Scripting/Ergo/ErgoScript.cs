using Ergo.Interpreter;
using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Runtime;
using Unconcern.Common;

namespace Fiero.Core
{
    public class ErgoScript : Script
    {
        public readonly ErgoVM VM;
        private readonly HashSet<EventHook> hooks = new();

        public readonly TermMarshallingContext MarshallingContext = new();
        public readonly Dictionary<EventHook, Hook> ErgoHooks = new();

        private volatile bool running = false;


        public ErgoScript(InterpreterScope scope)
        {
            VM = scope.Facade.BuildVM(
                scope.BuildKnowledgeBase(CompilerFlags.Default),
                DecimalType.CliDecimal);
            hooks = scope.GetLibrary<CoreLib>(ErgoModules.Core)
                .GetScriptSubscriptions(this)
                .Select(s => new EventHook(s.Module.GetOrThrow().Explain(), s.Functor.Explain()))
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
                subs.Add(routes[hook](this));
            return subs;
        }
    }
}
