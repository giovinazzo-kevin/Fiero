using Ergo.Interpreter;

namespace Fiero.Core.Ergo
{
    public class ErgoLayoutScript : ErgoScript
    {
        private readonly Dictionary<string, Func<LayoutGrid>> _componentDefs;

        public bool TryCreateComponent(string name, out LayoutGrid grid)
        {
            if(_componentDefs.TryGetValue(name, out var f))
            {
                grid = f();
                return true;
            }
            grid = default;
            return false;
        }

        public ErgoLayoutScript(InterpreterScope scope, Dictionary<string, Func<UIControl>> resolvers) : base(scope)
        {
            _componentDefs = ELLInterpreter.GetComponentDefinitions(VM, resolvers);
        }
    }
}
