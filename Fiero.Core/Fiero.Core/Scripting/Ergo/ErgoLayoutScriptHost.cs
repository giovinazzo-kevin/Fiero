using Ergo.Facade;
using Ergo.Interpreter;
using Ergo.Shell;

namespace Fiero.Core.Ergo
{
    public sealed class ErgoLayoutScriptHost(IAsyncInputReader inputReader, IServiceFactory fac)
        : ErgoScriptHostBase(inputReader, fac), IScriptHost<ErgoLayoutScript>
    {
        private Dictionary<string, Func<UIControl>> _controlResolvers;

        protected override ErgoLayoutScript MakeScript(InterpreterScope scope)
        {
            if(_controlResolvers is null)
            {
                _controlResolvers = ServiceFactory.GetAllInstances<IUIControlResolver>()
                   .Cast<IUIControlResolver>()
                   .DistinctBy(x => x.Type.Name)
                   .ToDictionary(x => x.Type.Name, x => (Func<UIControl>)x.ResolveUntyed);
            }
            return new(scope, _controlResolvers);
        }
    }
}
