namespace Fiero.Business
{
    public static class EffectExtensions
    {
        public static Temporary Temporary(this Effect e, int duration)
        {
            return new(e, duration);
        }

        public static GrantedOnUse GrantedOnUse(this Effect e)
        {
            return new(e);
        }
    }
}
