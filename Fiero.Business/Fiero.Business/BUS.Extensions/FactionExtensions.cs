namespace Fiero.Business
{
    public static class FactionExtensions
    {
        public static bool MayAttack(this StandingName a)
        {
            return (int)a <= (int)StandingName.Tolerated;
        }

        public static bool IsHostile(this StandingName a)
        {
            return (int)a < (int)StandingName.Tolerated;
        }

        public static bool MayFollow(this StandingName a)
        {
            return (int)a > (int)StandingName.Liked;
        }

        public static bool IsFriendly(this StandingName a)
        {
            return (int)a > (int)StandingName.Tolerated;
        }
    }
}
