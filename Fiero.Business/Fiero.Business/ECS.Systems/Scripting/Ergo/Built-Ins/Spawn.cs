using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using LightInject;
using System.Reflection;

namespace Fiero.Business;

[SingletonDependency]
public sealed class Spawn : SolverBuiltIn
{
    public readonly IServiceFactory Services;
    public readonly GameEntityBuilders Builders;

    private readonly Dictionary<string, MethodInfo> BuilderMethods;

    public Spawn(IServiceFactory services, GameEntityBuilders builders)
        : base("", new("spawn"), 2, ScriptingSystem.FieroModule)
    {
        Services = services;
        Builders = builders;
        BuilderMethods = Builders.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(EntityBuilder<>))
            .ToDictionary(m => m.Name.ToErgoCase());
    }

    public override IEnumerable<Evaluation> Apply(SolverContext context, SolverScope scope, ITerm[] args)
    {
        var spawned = new List<EcsEntity>();
        if (args[0] is List list)
        {
            var systems = Services.GetInstance<GameSystems>();
            // TODO: better way of determining floorID
            var player = systems.Render.Viewport.Following.V;
            var floorId = player?.FloorId() ?? default;
            var position = player?.Position() ?? default;
            foreach (var item in list.Contents)
            {
                if (item is not Dict dict)
                {
                    yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, WellKnown.Types.Dictionary, item);
                    yield break;
                }
                if (!dict.Functor.TryGetA(out var functor))
                {
                    yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, WellKnown.Types.Functor, item);
                    yield break;
                }
                if (!BuilderMethods.TryGetValue(functor.Explain(), out var method))
                {
                    yield return False();
                    yield break;
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
                        yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, p.ParameterType.Name, p);
                        yield break;
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
                        yield return False();
                        yield break;
                    }
                }
                else if (entity is Item i)
                {
                    if (!systems.TryPlace(floorId, i))
                    {
                        yield return False();
                        yield break;
                    }
                }
                else if (entity is Feature f)
                {
                    if (!systems.Dungeon.AddFeature(floorId, f))
                    {
                        yield return False();
                        yield break;
                    }
                }
                else if (entity is Tile t)
                {
                    systems.Dungeon.SetTileAt(floorId, t.Position(), t);
                }
            }
            systems.Render.CenterOn(player);
            if (args[1].Unify(new List(spawned.Select(x => new EntityAsTerm(x.Id, x.ErgoType())))).TryGetValue(out var subs))
            {
                yield return True(subs);
            }
            else
            {
                yield return True();
            }
            yield break;
        }
        else
        {
            foreach (var k in BuilderMethods.Keys)
            {
                if (args[0].Unify(new Atom(k)).TryGetValue(out var subs))
                    yield return True(subs);
            }
            yield break;
        }
        yield return False();
        yield break;
    }
}