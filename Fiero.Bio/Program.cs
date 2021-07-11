//using Fiero.Core;
//using LightInject;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Fiero.Bio
//{

//    class Program
//    {
//        static async Task Main(string[] args)
//        {
//            using var host = new GameHost();
//            var game = host.BuildGame<
//                BioGame,
//                FontName,
//                TextureName,
//                LocaleName,
//                SoundName,
//                ColorName,
//                ShaderName
//            >(Register);
//            await game.RunAsync(game.OffButton.Token);
//        }

//        static void Register(ServiceContainer services)
//        {
//            // Register all ECS components by reflection
//            var components = typeof(Program).Assembly.GetTypes()
//                .Where(t => !t.IsAbstract && (t.IsSubclassOf(typeof(Component)) || t.IsSubclassOf(typeof(Entity))));
//            foreach (var componentType in components) {
//                services.Register(componentType);
//            }
//            // Register all scenes via reflection
//            var scenes = typeof(Program).Assembly.GetTypes()
//                .Where(t => !t.IsAbstract && t.GetInterface(nameof(IGameScene)) != null);
//            foreach (var sceneType in scenes) {
//                services.Register(sceneType, new PerContainerLifetime());
//            }
//            // Register UI resolvers via reflection
//            var uiResolvers = typeof(Program).Assembly.GetTypes()
//                .Where(t => !t.IsAbstract)
//                .Select(t => (Type: t, Interfaces: t.GetInterfaces()
//                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUIControlResolver<>))))
//                .Where(t => t.Interfaces.Any());
//            foreach (var (resolverType, interfaceTypes) in uiResolvers) {
//                foreach (var type in interfaceTypes) {
//                    services.Register(type, resolverType);
//                }
//            }
//        }
//    }
//}
