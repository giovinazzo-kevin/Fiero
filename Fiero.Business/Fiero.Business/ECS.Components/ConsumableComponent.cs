namespace Fiero.Business
{
    public class ConsumableComponent : EcsComponent
    {
        public int RemainingUses { get; set; } = 1;
        public int MaximumUses { get; set; } = 1;
        public int UsesConsumedPerAction { get; set; } = 1;
        public bool ConsumedWhenEmpty { get; set; } = true;
    }
}
