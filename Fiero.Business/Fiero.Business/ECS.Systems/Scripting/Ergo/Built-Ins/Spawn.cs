using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using Fiero.Core;
using LightInject;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fiero.Business;

[SingletonDependency]
public sealed class Spawn : SolverBuiltIn
{
    public readonly IServiceFactory Services;
    public readonly GameEntityBuilders Builders;

    private readonly Dictionary<string, MethodInfo> BuilderMethods;

    public Spawn(IServiceFactory services, GameEntityBuilders builders)
        : base("", new("spawn"), 1, ErgoScriptingSystem.FieroModule)
    {
        Services = services;
        Builders = builders;
        BuilderMethods = Builders.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(EntityBuilder<>) && m.GetParameters().Where(x => !x.HasDefaultValue).Count() == 0)
            .ToDictionary(m => m.Name.ToErgoCase());
    }

    public override IEnumerable<Evaluation> Apply(SolverContext context, SolverScope scope, ITerm[] args)
    {
        if (!args[0].Matches(out string entityName) && args[0].IsGround)
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, WellKnown.Types.String, args[0]);
            yield break;
        }

        if (args[0].IsGround)
        {
            if (!BuilderMethods.TryGetValue(entityName, out var builderFunc))
            {
                yield return False();
                yield break;
            }
            var systems = Services.GetInstance<GameSystems>();
            // TODO: better way of determining floorID
            var player = systems.Render.Viewport.Following.V;
            var floorId = player?.FloorId() ?? default;
            var position = player?.Position() ?? default;

            var builder = (IEntityBuilder)builderFunc.Invoke(Builders, null);
            var entity = builder.Build();
            if (entity is PhysicalEntity e)
                e.Physics.Position = position;
            if (entity is Actor a)
            {
                if (systems.TrySpawn(floorId, a))
                    yield return True();
                else
                    yield return False();
            }
            else if (entity is Item i)
            {
                if (systems.TryPlace(floorId, i))
                    yield return True();
                else
                    yield return False();
            }
            else if (entity is Feature f)
            {
                if (systems.Dungeon.AddFeature(floorId, f))
                    yield return True();
                else
                    yield return False();
            }
            else if (entity is Tile t)
            {
                systems.Dungeon.SetTileAt(floorId, t.Position(), t);
                yield return True();
            }
            systems.Render.CenterOn(player);
        }
        else
        {
            foreach (var k in BuilderMethods.Keys)
            {
                if (args[0].Unify(new Atom(k)).TryGetValue(out var subs))
                    yield return True(subs);
            }
        }
    }
}