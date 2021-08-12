namespace Fiero.Business
{
    public class ActorStats
    {
        public int MaximumHealth { get; set; } = 10;
        public int Health { get; set; } = 10;
        public float HealthPercentage => Health / (float)MaximumHealth;
    }
}
