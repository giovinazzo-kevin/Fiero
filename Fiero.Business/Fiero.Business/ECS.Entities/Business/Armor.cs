using Fiero.Core;

namespace Fiero.Business
{
    public class Armor : Equipment
    {
        [RequiredComponent]
        public ArmorComponent ArmorProperties { get; private set; }
    }
}
