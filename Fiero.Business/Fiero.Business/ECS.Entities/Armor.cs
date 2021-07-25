using Fiero.Core;

namespace Fiero.Business
{
    public class Armor : Item
    {
        [RequiredComponent]
        public ArmorComponent ArmorProperties { get; private set; }
    }
}
