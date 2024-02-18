namespace Fiero.Business
{
    public class WeaponComponent : EcsComponent
    {
        public WeaponName Type { get; set; }
        public Dice BaseDamage { get; set; }
        public int SwingDelay { get; set; }

        public float DamagePerTurn => ((float)BaseDamage.Mean() * (100f / (SwingDelay + 100)));
    }
}
