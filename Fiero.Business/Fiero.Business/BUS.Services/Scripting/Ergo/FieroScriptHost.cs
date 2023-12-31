using Ergo.Facade;
using Ergo.Interpreter;
using Ergo.Shell;
using LightInject;

namespace Fiero.Business;

public sealed class FieroScriptHost(IAsyncInputReader inputReader, IServiceFactory fac)
    : ErgoScriptHost<ScriptName>(inputReader, fac)
{
    protected override ErgoFacade GetErgoFacade() => base
        .GetErgoFacade()
        .AddLibrary(() => new FieroLib());

    protected override ErgoScript MakeScript(InterpreterScope scope) => new FieroScript(scope);
}
