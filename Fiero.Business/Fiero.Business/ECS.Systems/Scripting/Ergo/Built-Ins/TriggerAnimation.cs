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
public sealed class TriggerAnimation : SolverBuiltIn
{
    private IServiceFactory _services;
    private readonly Dictionary<string, MethodInfo> Methods;

    public TriggerAnimation(IServiceFactory services)
        : base("", new("animate"), 3, ErgoScriptingSystem.FieroModule)
    {
        _services = services;
        Methods = typeof(Animation)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(m => m.ReturnType == typeof(Animation))
            .ToDictionary(m => m.Name.ToErgoCase());
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] args)
    {
        if (!args[0].Matches(out bool blocking))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, WellKnown.Types.Boolean, args[0]);
            yield break;
        }
        if (!args[1].Matches(out Coord pos))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, nameof(Coord), args[1]);
            yield break;
        }
        if (!args[2].IsAbstract<List>().TryGetValue(out var list))
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
        var renderSystem = _services.GetInstance<RenderSystem>();
        renderSystem.AnimateViewport(blocking, pos, animList.ToArray());
        yield return True();
    }
}
