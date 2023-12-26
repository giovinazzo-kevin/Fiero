namespace Fiero.Business
{
    public static partial class Data
    {
        public static class Player
        {
            public static readonly GameDatum<int> Id = new(nameof(Player), nameof(Id));
            public static readonly GameDatum<string> Name = new(nameof(Player), nameof(Name));
        }
    }
}
