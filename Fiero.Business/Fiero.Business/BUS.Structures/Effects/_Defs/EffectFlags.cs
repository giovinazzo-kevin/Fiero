namespace Fiero.Business
{
    public readonly struct EffectFlags
    {
        public readonly bool IsBuff;
        public readonly bool IsDebuff;
        public readonly bool IsPanicButton;
        public readonly bool IsOffensive;
        public readonly bool IsDefensive;

        public EffectFlags(bool buff, bool debuff, bool panic, bool offensive, bool defensive)
        {
            IsBuff = buff;
            IsDebuff = debuff;
            IsPanicButton = panic;
            IsOffensive = offensive;
            IsDefensive = defensive;
        }

        public EffectFlags Or(EffectFlags other) => new(
            IsBuff || other.IsBuff, 
            IsDebuff || other.IsDebuff, 
            IsPanicButton || other.IsPanicButton,
            IsOffensive || other.IsOffensive,
            IsDefensive || other.IsDefensive
        );
    }
}
