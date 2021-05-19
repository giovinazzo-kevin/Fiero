using Fiero.Core;

namespace Fiero.Business
{
    public class NpcComponent : Component
    {
        public NpcName Type { get; set; }

        public bool IsBoss => Type == NpcName.GreatKingRat;

    }
}
