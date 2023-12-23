using Ergo.Interpreter.Directives;
using Ergo.Interpreter.Libraries;
using Ergo.Lang.Ast;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Business
{
    internal partial class ScriptEffectLib(GameSystems systems, Atom module) : Library
    {
        public readonly record struct Context(ScriptEffect Source, Entity Owner, string Args);
        public readonly Dictionary<int, Context> Contexts = new();

        private readonly Atom _module = module;
        public override Atom Module => _module;
        public readonly GameSystems Systems = systems;
        public int CurrentOwner { get; private set; }
        public Context CurrentContext => Contexts[CurrentOwner];

        public void SetCurrentOwner(ScriptEffect source, Entity owner, string args)
        {
            Contexts[CurrentOwner = owner.Id] = new(source, owner, args);
        }

        public override IEnumerable<BuiltIn> GetExportedBuiltins()
        {
            yield return new End(this);
            yield return new Args(this);
            yield return new Owner(this);
            yield return new Subscribed(this);
        }
        public override IEnumerable<InterpreterDirective> GetExportedDirectives()
        {
            yield break;
        }
    }
}
