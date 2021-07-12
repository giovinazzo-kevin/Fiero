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
            var game = host.BuildGame<
                FieroGame, 
                FontName, 
                TextureName, 
                LocaleName, 
                SoundName, 
                ColorName, 
                ShaderName
            >(Register);
            await game.RunAsync(game.OffButton.Token);
        }

        static void Register(ServiceContainer services)
        {
        }
    }
}
