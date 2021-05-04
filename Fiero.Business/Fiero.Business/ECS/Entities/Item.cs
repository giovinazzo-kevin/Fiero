using Fiero.Core;
using System.ComponentModel.DataAnnotations;

namespace Fiero.Business
{
    public class Item : Drawable
    {
        [RequiredComponent]
        public ItemComponent Properties { get; private set; }
    }
}
