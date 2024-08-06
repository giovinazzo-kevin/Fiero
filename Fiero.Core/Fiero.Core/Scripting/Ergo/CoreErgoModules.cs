using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;

namespace Fiero.Core.Ergo
{
    public sealed class CoreErgoModules
    {
        public static readonly Atom Core = new(nameof(Core).ToErgoCase());
        public static readonly Atom Game = new(nameof(Game).ToErgoCase());
        public static readonly Atom Data = new(nameof(Data).ToErgoCase());
        public static readonly Atom Input = new(nameof(Input).ToErgoCase());
        public static readonly Atom Event = new(nameof(Event).ToErgoCase());
        public static readonly Atom Effect = new(nameof(Effect).ToErgoCase());
        public static readonly Atom Layout = new(nameof(Layout).ToErgoCase());
    }
}
