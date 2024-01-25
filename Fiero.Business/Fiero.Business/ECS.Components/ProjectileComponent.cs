namespace Fiero.Business
{
    public class ProjectileComponent : EcsComponent
    {
        public ProjectileName Name { get; set; }
        public int BaseDamage { get; set; } = 1;
        public int MaximumRange { get; set; } = 3;
        public float MulchChance { get; set; } = 0.25f;
        /// <summary>
        /// If true, throwing consumes charges. If false, throwing throws the item itself.
        /// </summary>
        public bool ThrowsUseCharges { get; set; }
        public bool Piercing { get; set; }
        public bool Directional { get; set; }
        public TrajectoryName Trajectory { get; set; }
        public string TrailSprite { get; set; }
    }
}
