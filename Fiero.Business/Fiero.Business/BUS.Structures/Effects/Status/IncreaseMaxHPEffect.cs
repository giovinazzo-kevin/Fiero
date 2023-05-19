namespace Fiero.Business
{
    public sealed class IncreaseMaxHPEffect : IncreaseMaxStatEffect
    {
        public IncreaseMaxHPEffect(Entity source, int amount) : base(source, amount, a => a.ActorProperties.Health) { }
        public override EffectName Name => EffectName.IncreaseMaxHP;
    }
}
