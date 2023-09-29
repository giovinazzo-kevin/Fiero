using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using LightInject;

namespace Fiero.Business;

[SingletonDependency]
public sealed class TriggerSound : SolverBuiltIn
{
    [Term(Functor = "sound_def", Marshalling = TermMarshalling.Named)]
    internal readonly record struct SoundDefStub()
    {
        [Term(Marshalling = TermMarshalling.Positional)]
        public Coord Position { get; init; }
        public float Pitch { get; init; } = 1f;
        public float Volume { get; init; } = 2f;
        public bool Relative { get; init; } = true;
    };

    private IServiceFactory _services;

    public TriggerSound(IServiceFactory services)
        : base("", new("play"), 1, ErgoScriptingSystem.SoundModule)
    {
        _services = services;
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] args)
    {
        if (!args[0].IsAbstract<Dict>().TryGetValue(out var dict))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, WellKnown.Types.Dictionary, args[0]);
            yield break;
        }
        if (!dict.Functor.TryGetA(out var functor))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, WellKnown.Types.Functor, args[0]);
            yield break;
        }
        if (!functor.Matches(out SoundName sound))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, nameof(SoundName), args[0]);
            yield break;
        }
        if (!args[0].Matches(out SoundDefStub stub, matchFunctor: false))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, nameof(SoundDefStub), args[0]);
        }
        var pos = stub.Position;
        if (stub.Relative)
        {
            var center = _services.GetInstance<GameSystems>().Render.Viewport.Following.V.Position();
            pos -= center;
        }
        _services.GetInstance<GameResources>().Sounds.Get(sound, pos, stub.Volume, stub.Pitch).Play();
        yield return True();
    }
}
