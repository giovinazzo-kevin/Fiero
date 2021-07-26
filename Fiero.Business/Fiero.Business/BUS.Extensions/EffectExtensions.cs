namespace Fiero.Business
{
    public static class EffectExtensions
    {
        public static TemporaryEffect Temporary(this Effect e, int duration)
        {
            return new(e, duration);
        }

        public static GrantOnUseEffect GrantedOnUse(this Effect e)
        {
            return new(e);
        }
    }
}
