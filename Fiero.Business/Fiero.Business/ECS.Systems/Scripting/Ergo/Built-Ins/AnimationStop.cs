using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using LightInject;

namespace Fiero.Business;

[SingletonDependency]
public sealed class AnimationStop(IServiceFactory services)
    : BuiltIn("", new("stop_animation"), 1, ScriptingSystem.AnimationModule)
{
    private readonly IServiceFactory _services = services;

    public override ErgoVM.Op Compile()
    {
        var render = _services.GetInstance<RenderSystem>();
        return vm =>
        {
            var args = vm.Args;
            if (!args[0].Matches(out int id))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Integer, args[0]);
                return;
            }
            render.StopAnimation(id);
        };
    }
}
