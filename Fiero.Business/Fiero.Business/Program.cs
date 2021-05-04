using Fiero.Core;
using LightInject;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Fiero.Business
{
    class Program
    {
        public static readonly OffButton OffButton = new();

        static async Task Main(string[] args)
        {
            using var host = new GameHost();
            var game = host.BuildGame<FieroGame, FontName, TextureName, LocaleName, SoundName, ColorName>(Register);
            await game.InitializeAsync();
            game.Run(OffButton.Token);
        }

        static void Register(ServiceContainer services)
        {
            services.Register<GameGlossaries>(new PerContainerLifetime());
            services.Register<GameDialogues>(new PerContainerLifetime());

            // Register ECS entities and systems
            services.Register<FloorSystem>(new PerContainerLifetime());
            services.Register<ActionSystem>(new PerContainerLifetime());
            services.Register<RenderSystem>(new PerContainerLifetime());
            services.Register<DialogueSystem>(new PerContainerLifetime());
            services.Register<FactionSystem>(new PerContainerLifetime());

            // Register all ECS components via reflection
            var components = typeof(Program).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && (t.IsSubclassOf(typeof(Component)) || t.IsSubclassOf(typeof(Entity))));
            foreach (var componentType in components) {
                services.Register(componentType);
            }
            // Register off button via reflection so that the game can be closed as a service
            services.Register(_ => OffButton);
            // Register all scenes via reflection
            var scenes = typeof(Program).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.GetInterface(nameof(IGameScene)) != null);
            foreach (var sceneType in scenes) {
                services.Register(sceneType, new PerContainerLifetime());
            }
        }
    }
}
