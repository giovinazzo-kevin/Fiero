using Ergo.Interpreter;
using Ergo.Shell;

namespace Fiero.Core.Ergo
{
    public class DefaultErgoScriptHost : ErgoScriptHostBase, IScriptHost<ErgoScript>
    {
        public DefaultErgoScriptHost(IAsyncInputReader inputReader, IServiceFactory fac) : base(inputReader, fac) { }
        protected override ErgoScript MakeScript(InterpreterScope scope) => new(scope);
    }
}
