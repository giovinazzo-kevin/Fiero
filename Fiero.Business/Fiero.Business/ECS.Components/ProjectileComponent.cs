namespace Fiero.Business
{
    public class ProjectileComponent : EcsComponent
    {
        public ProjectileName Name { get; set; }
        public int BaseDamage { get; set; } = 1;
        public int MaximumRange { get; set; } = 3;
        public float MulchChance { get; set; } = 0.25f;
        public bool ThrowsUseCharges { get; set; }
        public bool Piercing { get; set; }
        public bool Directional { get; set; }
        public TrajectoryName Trajectory { get; set; }
    }
}
