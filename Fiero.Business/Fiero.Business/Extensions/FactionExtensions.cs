namespace Fiero.Business
{
    public static class FactionExtensions
    {
        public static bool MayAttack(this Relationship a)
        {
            return (int)a.Standing <= (int)StandingName.Tolerated;
        }

        public static bool MayTarget(this Relationship a)
        {
            return (int)a.Standing < (int)StandingName.Tolerated;
        }

        public static bool MayFollow(this Relationship a)
        {
            return (int)a.Standing > (int)StandingName.Liked;
        }

        public static bool MayHelp(this Relationship a)
        {
            return (int)a.Standing > (int)StandingName.Tolerated;
        }
    }
}
