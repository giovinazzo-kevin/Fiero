using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Business;

[SingletonDependency]
public sealed class CenterViewOn : BuiltIn
{
    private readonly MetaSystem meta;
    public CenterViewOn(MetaSystem meta)
        : base("", new("center_view_on"), 1, FieroLib.Modules.Fiero)
    {
        this.meta = meta;
    }

    public override ErgoVM.Op Compile() => vm =>
    {
        var arg = vm.Args[0];
        var render = meta.Get<RenderSystem>();
        if (arg is EntityAsTerm e && e.GetProxy<Actor>().TryGetValue(out var a))
        {
            render.CenterOn(a);
        }
        else if (arg.Matches(out Coord coord))
        {
            render.CenterOn(coord);
        }
        else
        {
            vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, typeof(Coord), arg.Explain());
            return;
        }
    };
}
