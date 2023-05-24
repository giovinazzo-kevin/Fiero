﻿using Fiero.Core;
using LightInject;
using System.Text;
using System.Threading.Tasks;

namespace Fiero.Business
{
    class Program
    {
        static async Task Main(string[] args)
        {
            CodePagesEncodingProvider.Instance.GetEncoding(437);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

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
