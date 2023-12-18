using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using LightInject;

namespace Fiero.Business;

[SingletonDependency]
public sealed class Despawn(IServiceFactory services) : BuiltIn("", new Atom("despawn"), 1, ScriptingSystem.FieroModule)
{
    private readonly IServiceFactory _services = services;

    public override ErgoVM.Op Compile()
    {
        var action = _services.GetInstance<ActionSystem>();
        var entities = _services.GetInstance<GameEntities>();
        return vm =>
        {
            if (vm.Arg(0).Matches<int>(out var id))
            {
                if (entities.TryGetProxy<Entity>(id, out var entity))
                {
                    action.Despawn(entity);
                }
            }
            else vm.Fail();
        };
    }
}
