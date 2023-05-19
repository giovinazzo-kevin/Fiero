namespace Fiero.Business
{
    public sealed class IncreaseMaxMPEffect : IncreaseMaxStatEffect
    {
        public IncreaseMaxMPEffect(Entity source, int amount) : base(source, amount, a => a.ActorProperties.Magic) { }
        public override EffectName Name => EffectName.IncreaseMaxMP;
    }
}
