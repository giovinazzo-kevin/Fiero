using Ergo.Lang;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using Fiero.Core.Ergo.Libraries.Core;

namespace Fiero.Core.Ergo.Libraries.Core.Game;

[SingletonDependency]
public sealed class SetScene : BuiltIn
{
    private readonly GameDirector director;
    public SetScene(GameDirector director)
        : base("", new("set_scene_state"), 1, CoreErgoModules.Game)
    {
        this.director = director;
    }

    public override ErgoVM.Op Compile() => vm =>
    {
        var scene = director.CurrentScene;
        var state = TermMarshall.FromTerm(vm.Args[0], scene.State.GetType());
        if (!scene.TrySetState(state))
        {
            vm.Fail();
            return;
        }
    };
}
