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
        static async Task Main(string[] args)
        {
            using var host = new GameHost();
            var game = host.BuildGame<FieroGame, FontName, TextureName, LocaleName, SoundName, ColorName>(Register);
            await game.InitializeAsync();
            game.Run(game.OffButton.Token);
        }

        static void Register(ServiceContainer services)
        {
            // Register UI resolvers by reflection
            var resolvers = typeof(Program).Assembly.GetTypes()
                .Where(t => !t.IsAbstract)
                .Select(t => (Type: t, Interfaces: t.GetInterfaces()
                        .Where(i => i.IsGenericType 
                            && i.GetGenericTypeDefinition() == typeof(IUIControlResolver<>))))
                .Where(t => t.Interfaces.Any());
            foreach (var (resolverType, interfaceTypes) in resolvers) {
                foreach (var type in interfaceTypes) {
                    services.Register(type, resolverType);
                }
            }

            // Register services
            services.Register<GameGlossaries>(new PerContainerLifetime());
            services.Register<GameDialogues>(new PerContainerLifetime());

            // Register ECS entities and systems
            services.Register<FloorSystem>(new PerContainerLifetime());
            services.Register<ActionSystem>(new PerContainerLifetime());
            services.Register<RenderSystem>(new PerContainerLifetime());
            services.Register<DialogueSystem>(new PerContainerLifetime());
            services.Register<FactionSystem>(new PerContainerLifetime());

            // Register all ECS components by reflection
            var components = typeof(Program).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && (t.IsSubclassOf(typeof(Component)) || t.IsSubclassOf(typeof(Entity))));
            foreach (var componentType in components) {
                services.Register(componentType);
            }
            // Register all scenes via reflection
            var scenes = typeof(Program).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.GetInterface(nameof(IGameScene)) != null);
            foreach (var sceneType in scenes) {
                services.Register(sceneType, new PerContainerLifetime());
            }
        }
    }
}
