using Fiero.Core;

namespace Fiero.Business
{
    public class NpcComponent : EcsComponent
    {
        public NpcName Type { get; set; }
        public RaceName Race { get; set; }

        public bool IsBoss => Type == NpcName.GreatKingRat;

    }
}
