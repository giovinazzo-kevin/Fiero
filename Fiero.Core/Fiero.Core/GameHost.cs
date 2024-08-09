using Ergo.Shell;
using Fiero.Core.Ergo;
using System.Reflection;
using Unconcern.Common;

namespace Fiero.Core
{
    public class GameHost : IDisposable
    {
        private readonly ServiceContainer _ioc;

        public GameHost()
        {
            _ioc = new ServiceContainer();
        }

        public TGame BuildGame<TGame>(Action<ServiceContainer> configureServices = null)
            where TGame : Game
        {
            _ioc.Register<IServiceFactory>(_ => _ioc.BeginScope());
            _ioc.Register<EventBus>(new PerContainerLifetime());
            _ioc.Register<IScriptHost<ErgoScript>, DefaultErgoScriptHost>(new PerContainerLifetime());
            _ioc.Register<IAsyncInputReader, ErgoInputReader>(new PerContainerLifetime());
            _ioc.Register<TGame>(new PerContainerLifetime());

            var singletons = Assembly.GetEntryAssembly().GetTypes().Concat(Assembly.GetExecutingAssembly().GetTypes())
                .Distinct()
                .Select(t => (Type: t, Attr: t.GetCustomAttribute<SingletonDependencyAttribute>()))
                .Where(x => x.Attr != null && !x.Type.IsAbstract && x.Type.IsClass);
            foreach (var (type, attr) in singletons)
            {
                foreach (var inter in attr.InterfaceTypes)
                    _ioc.Register(inter, type, new PerContainerLifetime());
                if (attr.InterfaceTypes.Length == 0)
                    _ioc.Register(type, new PerContainerLifetime());
            }
            var transients = Assembly.GetEntryAssembly().GetTypes()
                .Select(t => (Type: t, Attr: t.GetCustomAttribute<TransientDependencyAttribute>()))
                .Where(x => x.Attr != null && !x.Type.IsAbstract && x.Type.IsClass);
            foreach (var (type, attr) in transients)
            {
                foreach (var inter in attr.InterfaceTypes)
                    _ioc.Register(inter, type);
                if (attr.InterfaceTypes.Length == 0)
                    _ioc.Register(type);
            }

            configureServices?.Invoke(_ioc);
            _ioc.Compile();
            return _ioc.GetInstance<TGame>();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _ioc.Dispose();
        }
    }
}
