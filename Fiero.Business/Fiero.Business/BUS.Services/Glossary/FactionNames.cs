namespace Fiero.Business
{
    public readonly struct FactionNames
    {
        public readonly string[] Tier1;
        public readonly string[] Tier2;
        public readonly string[] Tier3;
        public readonly string[] Tier4;
        public readonly string[] Tier5;

        public FactionNames(string[] t1, string[] t2, string[] t3, string[] t4, string[] t5)
        {
            Tier1 = t1;
            Tier2 = t2;
            Tier3 = t3;
            Tier4 = t4;
            Tier5 = t5;
        }
    }
}
