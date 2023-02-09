using Fiero.Core;

namespace Fiero.Business
{
    public class Corpse : Item
    {
        [RequiredComponent]
        public CorpseComponent CorpseProperties { get; set; }
    }
}
