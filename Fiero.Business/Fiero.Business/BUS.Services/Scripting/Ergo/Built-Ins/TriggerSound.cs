﻿using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using LightInject;

namespace Fiero.Business;

[SingletonDependency]
public sealed class TriggerSound : BuiltIn
{
    [Term(Functor = "sound_def", Marshalling = TermMarshalling.Named)]
    internal readonly record struct SoundDefStub()
    {
        [Term(Marshalling = TermMarshalling.Positional)]
        public FloorId FloorId { get; init; }
        [Term(Marshalling = TermMarshalling.Positional)]
        public Coord Position { get; init; }
        public float Pitch { get; init; } = 1f;
        public float Volume { get; init; } = 100f;
    };

    private IServiceFactory _services;

    public TriggerSound(IServiceFactory services)
        : base("", new("play_sound"), 1, FieroLib.Modules.Sound)
    {
        _services = services;
    }

    public override ErgoVM.Op Compile()
    {
        var systems = _services.GetInstance<MetaSystem>();
        var resources = _services.GetInstance<GameResources>();
        return vm =>
        {
            var args = vm.Args;
            if (!args[0].IsAbstract<Dict>().TryGetValue(out var dict))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Dictionary, args[0]);
                return;
            }
            if (!dict.Functor.TryGetA(out var functor))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Functor, args[0]);
                return;
            }
            if (!functor.Match(out SoundName sound))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(SoundName), args[0]);
                return;
            }
            if (!args[0].Match(out SoundDefStub stub, matchFunctor: false))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(SoundDefStub), args[0]);
                return;
            }
            var player = systems.Get<RenderSystem>().Viewport.Following.V;
            var pos = stub.Position;
            var isRelative = !stub.FloorId.Equals(default);
            if (isRelative)
            {
                var center = player.Position();
                pos -= center;
            }
            if (!isRelative || stub.FloorId == player.FloorId())
            {
                resources.Sounds
                    .Get(sound, pos, stub.Volume, stub.Pitch).Play();
            }
        };
    }
}
