using Ergo.Facade;
using Ergo.Interpreter;
using Ergo.Shell;
using Fiero.Core.Ergo;
using LightInject;

namespace Fiero.Business;

public sealed class FieroScriptHost(IAsyncInputReader inputReader, IServiceFactory fac)
    : ErgoScriptHost(inputReader, fac)
{
    protected override ErgoFacade GetErgoFacade() => base.GetErgoFacade()
        .AddLibrary(fac.GetInstance<FieroLib>)
        ;

    protected override InterpreterScope GetCoreScope() => base.GetCoreScope();

    protected override ErgoScript MakeScript(InterpreterScope scope) => new FieroScript(scope);
}
