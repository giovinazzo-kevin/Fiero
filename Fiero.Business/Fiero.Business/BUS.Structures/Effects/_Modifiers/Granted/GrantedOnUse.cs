﻿namespace Fiero.Business
{
    public class GrantedOnUse : UseEffect
    {
        public override string DisplayName => $"$Effect.{Source.Name}$";
        public override string DisplayDescription => "$Effect.GrantedOnUse$";
        public override EffectName Name => Source.Name;

        public GrantedOnUse(EffectDef source) : base(source)
        {
        }

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            Source.Resolve().Start(systems, target);
        }
    }
}