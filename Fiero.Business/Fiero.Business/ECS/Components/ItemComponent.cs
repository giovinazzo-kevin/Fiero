using Fiero.Core;

namespace Fiero.Business
{
    public class ItemComponent : Component
    {
        public int Rarity { get; set; }
        public bool Identified { get; set; }
        public string UnidentifiedName { get; set; }
    }
}
