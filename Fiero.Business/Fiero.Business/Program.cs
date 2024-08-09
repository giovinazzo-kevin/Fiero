using Fiero.Core.Ergo;
using LightInject;
using System.Text;

namespace Fiero.Business
{
    class Program
    {
        static async Task Main(string[] args)
        {
            CodePagesEncodingProvider.Instance.GetEncoding(437);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using var host = new GameHost();
            var game = host.BuildGame<FieroGame>(Register);
            await game.RunAsync(game.OffButton.Token);
        }

        static void Register(ServiceContainer services)
        {
            services.Register<IScriptHost<FieroScript>, FieroScriptHost>(new PerContainerLifetime());
            services.Override<IScriptHost<ErgoScript>, FieroScriptHost>(new PerContainerLifetime());
            services.Register<IScriptHost<ErgoLayoutScript>, ErgoLayoutScriptHost>(new PerContainerLifetime());
        }
    }
}
