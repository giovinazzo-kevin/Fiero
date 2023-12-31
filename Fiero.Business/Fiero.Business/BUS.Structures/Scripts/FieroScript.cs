using Ergo.Interpreter;
using Ergo.Interpreter.Libraries;
using Ergo.Runtime;

namespace Fiero.Business;

public class FieroScript : ErgoScript
{
    public readonly Hook EffectBeganHook;
    public readonly Hook EffectEndedHook;
    public readonly ErgoVM.Op EffectBeganOp;
    public readonly ErgoVM.Op EffectEndedOp;
    public FieroScript(InterpreterScope scope) : base(scope)
    {
        EffectBeganHook = new(new(new("began"), 1, ErgoModules.Effect, default));
        EffectEndedHook = new(new(new("ended"), 1, ErgoModules.Effect, default));
        EffectBeganOp = EffectBeganHook.Compile(throwIfNotDefined: true);
        EffectEndedOp = EffectEndedHook.Compile(throwIfNotDefined: true);
    }
}