using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using Fiero.Core;
using LightInject;
using static Ergo.Runtime.ErgoVM;

namespace Fiero.Core.Ergo.Libraries.Core.Sound;

[SingletonDependency]
public sealed class PlaySound : BuiltIn
{
    private readonly GameSounds _sounds;
    public PlaySound(GameSounds sounds)
        : base("", new("play_sound"), 4, CoreErgoModules.Sound)
    {
        _sounds = sounds;
    }

    public override Op Compile()
    {
        return vm =>
        {
            var args = vm.Args;
            if (!args[0].Match(out string sound))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(String), args[0]);
                return;
            }
            var (pos, volume, pitch) = (default(Coord?), default(float?), default(float?));
            if (args[1] is not Variable && !args[1].Match(out pos))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(Coord), args[1]);
                return;
            }
            if (args[2] is not Variable && !args[2].Match(out volume))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(Single), args[2]);
                return;
            }
            if (args[3] is not Variable && !args[3].Match(out pitch))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(Single), args[3]);
                return;
            }
            _sounds.Get(sound.ToCSharpCase(), pos, volume ?? 25, pitch ?? 1).Play();
        };
    }
}
