using LightInject;
using System;
using System.Threading;

namespace Fiero.Core
{
    public class GameHost : IDisposable
    {
        private readonly ServiceContainer _ioc;

        public GameHost()
        {
            _ioc = new ServiceContainer();
        }

        public TGame BuildGame<TGame, TFonts, TTextures, TLocales, TSounds, TColors, TShaders>(Action<ServiceContainer> configureServices = null)
            where TGame : Game<TFonts, TTextures, TLocales, TSounds, TColors, TShaders>
            where TFonts : struct, Enum
            where TTextures : struct, Enum
            where TLocales : struct, Enum
            where TSounds : struct, Enum
            where TColors : struct, Enum
            where TShaders : struct, Enum
        {
            _ioc.Register<IServiceFactory>(_ => _ioc.BeginScope());
            _ioc.Register<OffButton>(new PerContainerLifetime());
            _ioc.Register<GameLoop>(new PerContainerLifetime());
            _ioc.Register<GameInput>(new PerContainerLifetime());
            _ioc.Register<GameFonts<TFonts>>(new PerContainerLifetime());
            _ioc.Register<GameTextures<TTextures>>(new PerContainerLifetime());
            _ioc.Register<GameShaders<TShaders>>(new PerContainerLifetime());
            _ioc.Register<GameColors<TColors>>(new PerContainerLifetime());
            _ioc.Register<GameSounds<TSounds>>(new PerContainerLifetime());
            _ioc.Register<GameSprites<TTextures>>(new PerContainerLifetime());
            _ioc.Register<GameDataStore>(new PerContainerLifetime());
            _ioc.Register<GameDirector>(new PerContainerLifetime());
            _ioc.Register<GameEntities>(new PerContainerLifetime());
            _ioc.Register<GameLocalizations<TLocales>>(new PerContainerLifetime());
            _ioc.Register<GameUI>(new PerContainerLifetime());
            _ioc.Register<TGame>(new PerContainerLifetime());
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
