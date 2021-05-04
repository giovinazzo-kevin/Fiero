namespace Fiero.Business
{
    public static class FactionExtensions
    {
        public static bool MayAttack(this StandingName a)
        {
            return (int)a <= (int)StandingName.Neutral;
        }

        public static bool MayTarget(this StandingName a)
        {
            return (int)a < (int)StandingName.Neutral;
        }

        public static bool MayFollow(this StandingName a)
        {
            return (int)a > (int)StandingName.Liked;
        }

        public static bool MayHelp(this StandingName a)
        {
            return (int)a > (int)StandingName.Neutral;
        }
    }
}
