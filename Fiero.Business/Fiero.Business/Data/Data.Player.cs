using Fiero.Core;

namespace Fiero.Business
{
    public static partial class Data
    {
        public static class Player
        {
            public static readonly GameDatum<string> Name = new(nameof(Player) + nameof(Name));
            public static readonly GameDatum<Actor> KilledBy = new(nameof(Player) + nameof(KilledBy));
        }

    }
}
