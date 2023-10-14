using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using LightInject;
using System.Reflection;

namespace Fiero.Business;

public abstract class TriggerAnimationBase : SolverBuiltIn
{
    protected readonly IServiceFactory Services;
    private readonly Dictionary<string, MethodInfo> Methods;

    protected bool IsBlocking { get; set; } = false;

    public TriggerAnimationBase(IServiceFactory services, string name)
        // play(pos or physical_entity, anim_list, IdsList).
        : base("", new(name), 3, ScriptingSystem.AnimationModule)
    {
        Services = services;
        Methods = typeof(Animation)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(m => m.ReturnType == typeof(Animation))
            .ToDictionary(m => m.Name.ToErgoCase());
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] args)
    {
        var at = default(Either<Location, PhysicalEntity>);
        if (args[0].Matches(out Location location))
        {
            at = location;
        }
        else if (args[0].IsAbstract<EntityAsTerm>().TryGetValue(out var entityAsTerm)
            && entityAsTerm.GetProxy().TryGetValue(out var proxy)
            && proxy is PhysicalEntity pe)
        {
            at = pe;
        }
        else
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, nameof(Location), args[0]);
            yield break;
        }
        if (!args[1].IsAbstract<List>().TryGetValue(out var list))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, WellKnown.Types.List, args[2]);
            yield break;
        }
        var animList = new List<Animation>();
        foreach (var anim in list.Contents)
        {
            if (!anim.IsAbstract<Dict>().TryGetValue(out var dict))
            {
                yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, WellKnown.Types.Dictionary, anim);
                yield break;
            }
            if (!dict.Functor.TryGetA(out var functor))
            {
                yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, WellKnown.Types.Functor, anim);
                yield break;
            }
            if (!Methods.TryGetValue(functor.Explain(), out var method))
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
            animList.Add((Animation)method.Invoke(null, newParams));
        }
        var renderSystem = Services.GetInstance<RenderSystem>();
        var lastId = renderSystem.AnimateViewport(IsBlocking, at, animList.ToArray());
        var idList = Enumerable.Range(lastId - animList.Count, animList.Count);
        if (args[2].Unify(new List(idList.Select(x => new Atom(x + 1)).Cast<ITerm>())).TryGetValue(out var subs))
        {
            yield return True(subs);
            yield break;
        }
        else
        {
            yield return False();
            yield break;
        }
    }
}
