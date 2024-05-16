using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Core.Ergo.Libraries.Core.Input;

[SingletonDependency]
public class SimulateKey(GameInput input)
    : BuiltIn("Simulates pressing a key for use in macros and demos", new Atom("simulate_key"), 1, ErgoModules.Input)
{
    public readonly GameInput Input = input;

    public override ErgoVM.Op Compile() => vm =>
    {
        if (!vm.Arg(0).Matches<InputSystem.KeyEvent>(out var keyEvt, mode: TermMarshalling.Named))
        {
            vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(InputSystem.KeyEvent), vm.Arg(0).Explain());
            return;
        }
        switch (keyEvt.Type)
        {
            case InputSystem.KeyEventType.Pressed:
                Input.SimulateKeyPress(keyEvt.Key);
                break;
            case InputSystem.KeyEventType.Released:
                Input.SimulateKeyRelease(keyEvt.Key);
                break;
        }
    };
}