namespace Fiero.Business
{
    public readonly struct EffectFlags
    {
        public readonly bool IsBuff;
        public readonly bool IsDebuff;
        public readonly bool IsPanicButton;

        public EffectFlags(bool buff, bool debuff, bool panic)
        {
            IsBuff = buff;
            IsDebuff = debuff;
            IsPanicButton = panic;
        }

        public EffectFlags Or(EffectFlags other) => new(
            IsBuff || other.IsBuff, 
            IsDebuff || other.IsDebuff, 
            IsPanicButton || other.IsPanicButton
        );
    }
}
