using Fiero.Core;
using System;

namespace Fiero.Business
{
    public class SpellComponent : EcsComponent
    {
        public SpellName Name { get; set; }
        public TargetingShape TargetingShape { get; set; }
        public Func<MetaSystem, Actor, PhysicalEntity, bool> TargetingFilter { get; set; }
        public int BaseDamage { get; set; }
        public int CastDelay { get; set; }
        // TODO: Targeting filter
    }
}
