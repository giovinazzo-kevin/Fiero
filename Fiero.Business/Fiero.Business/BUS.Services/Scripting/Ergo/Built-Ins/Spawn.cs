using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using LightInject;
using System.Reflection;

namespace Fiero.Business;

[SingletonDependency]
public sealed class Spawn : BuiltIn
{
    public readonly IServiceFactory Services;
    public readonly GameEntityBuilders Builders;
    public readonly GameEntities Entities;

    private readonly Dictionary<string, MethodInfo> BuilderMethods;

    public Spawn(IServiceFactory services, GameEntityBuilders builders, GameEntities entities)
        : base("", new("spawn"), 2, FieroLib.Modules.Fiero)
    {
        Services = services;
        Builders = builders;
        Entities = entities;
        BuilderMethods = Builders.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(EntityBuilder<>))
            .ToDictionary(m => m.Name.ToErgoCase());
    }

    public override ErgoVM.Op Compile()
    {
        return vm =>
        {
            var args = vm.Args;
            var spawned = new List<EcsEntity>();
            if (args[0] is List list)
            {
                var systems = Services.GetInstance<MetaSystem>();
                // TODO: better way of determining floorID
                var player = systems.Get<RenderSystem>().Viewport.Following.V;
                var floorId = player?.FloorId() ?? default;
                var position = player?.Position() ?? default;
                foreach (var item in list.Contents)
                {
                    if (item is not Dict dict)
                    {
                        vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Dictionary, item);
                        return;
                    }
                    if (!dict.Functor.TryGetA(out var functor))
                    {
                        vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Functor, item);
                        return;
                    }
                    if (!BuilderMethods.TryGetValue(functor.Explain(), out var method))
                    {
                        vm.Fail();
                        return;
                    }
                    var oldParams = method.GetParameters();
                    var newParams = new object[oldParams.Length];
                    for (int i = 0; i < oldParams.Length; i++)
                    {
                        var p = oldParams[i];
                        if (dict.Dictionary.TryGetValue(new Atom(p.Name.ToErgoCase()), out var value)
                        && TermMarshall.FromTerm(value, p.ParameterType) is { } val)
                        {
                            newParams[i] = val;
                        }
                        else if (p.HasDefaultValue)
                        {
                            newParams[i] = p.DefaultValue;
                        }
                        else
                        {
                            vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, p.ParameterType.Name, p);
                            return;
                        }
                    }
                    var builder = (IEntityBuilder)method.Invoke(Builders, newParams);
                    var entity = builder.Build();
                    spawned.Add(entity);
                    if (entity is PhysicalEntity e)
                        e.Physics.Position = position;
                    if (entity is Actor a)
                    {
                        if (!systems.TrySpawn(floorId, a))
                        {
                            vm.Fail();
                            return;
                        }
                    }
                    else if (entity is Item i)
                    {
                        if (!systems.TryPlace(floorId, i))
                        {
                            vm.Fail();
                            return;
                        }
                    }
                    else if (entity is Feature f)
                    {
                        if (!systems.Get<DungeonSystem>().AddFeature(floorId, f))
                        {
                            vm.Fail();
                            return;
                        }
                    }
                    else if (entity is Tile t)
                    {
                        systems.Get<DungeonSystem>().SetTileAt(floorId, t.Position(), t);
                    }
                }
                systems.Get<RenderSystem>().CenterOn(player);
                vm.SetArg(0, args[1]);
                vm.SetArg(1, new List(spawned.Select(x => new EntityAsTerm(x.Id, x.ErgoType(), Entities))));
                ErgoVM.Goals.Unify2(vm);
                vm.Success();
                return;
            }
            else
            {
                int k = 0;
                NextKey(vm);
                void NextKey(ErgoVM vm)
                {
                    var key = BuilderMethods.Keys.ElementAt(k++);
                    if (k < BuilderMethods.Keys.Count)
                        vm.PushChoice(NextKey);
                    vm.SetArg(1, new Atom(k));
                    ErgoVM.Goals.Unify2(vm);
                }
                return;
            }
        };
    }
}