using Ergo.Interpreter;
using Ergo.Runtime;

namespace Fiero.Core
{
    public class ErgoScript(InterpreterScope scope) : Script
    {
        public readonly ErgoVM VM = scope.Facade.BuildVM(
            scope.BuildKnowledgeBase(CompilerFlags.Default),
            DecimalType.CliDecimal);
    }
}
