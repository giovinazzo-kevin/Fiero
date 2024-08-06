using Ergo.Facade;
using Ergo.Interpreter;
using Ergo.Shell;
using Fiero.Core.Ergo;
using LightInject;

namespace Fiero.Business;

public sealed class FieroScriptHost(IAsyncInputReader inputReader, IServiceFactory fac)
    : ErgoScriptHostBase(inputReader, fac), IScriptHost<ErgoScript>, IScriptHost<FieroScript>
{
    protected override ErgoFacade GetErgoFacade() => base.GetErgoFacade()
        .AddLibrary(ServiceFactory.GetInstance<FieroLib>);
    protected override FieroScript MakeScript(InterpreterScope scope) => new(scope);
}
