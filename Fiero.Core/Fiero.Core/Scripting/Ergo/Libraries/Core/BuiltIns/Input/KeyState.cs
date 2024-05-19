using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Core.Ergo.Libraries.Core.Input;

public class KeyState(GameInput input)
    : BuiltIn("Gets whether a key is pressed, down, released or up", new Atom("key_state"), 2, CoreErgoModules.Input)
{
    private static readonly Atom pressed = new(nameof(pressed));
    private static readonly Atom released = new(nameof(released));
    private static readonly Atom down = new(nameof(down));
    private static readonly Atom up = new(nameof(up));
    public readonly GameInput Input = input;

    public override ErgoVM.Op Compile() => vm =>
    {
        if (!vm.Arg(0).Match<VirtualKeys>(out var vk))
        {
            vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(VirtualKeys), vm.Arg(0).Explain());
            return;
        }
        if (Input.IsKeyDown(vk))
        {
            vm.SetArg(0, down);
        }
        else if (Input.IsKeyUp(vk))
        {
            vm.SetArg(0, up);
        }
        else if (Input.IsKeyPressed(vk))
        {
            vm.SetArg(0, pressed);
        }
        else if (Input.IsKeyReleased(vk))
        {
            vm.SetArg(0, released);
        }
        ErgoVM.Goals.Unify2(vm);
    };
}