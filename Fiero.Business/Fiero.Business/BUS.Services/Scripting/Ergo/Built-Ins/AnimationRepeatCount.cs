using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using LightInject;

namespace Fiero.Business;

[SingletonDependency]
public sealed class AnimationRepeatCount(IServiceFactory services) : BuiltIn("", new("repeat"), 2, FieroLib.Modules.Animation)
{
    private readonly IServiceFactory _services = services;

    public override ErgoVM.Op Compile()
    {
        var render = _services.GetInstance<RenderSystem>();
        return vm =>
        {
            var args = vm.Args;
            if (!args[0].Match(out int id))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Integer, args[0]);
                return;
            }
            if (!args[1].Match(out int times))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Integer, args[1]);
                return;
            }
            render.AlterAnimation(id, a => a.RepeatCount = times);
        };
    }
}
