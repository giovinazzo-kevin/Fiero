using Fiero.Core;
using System.ComponentModel.DataAnnotations;

namespace Fiero.Business
{
    public class Item : Drawable
    {
        [RequiredComponent]
        public ItemComponent ItemProperties { get; private set; }

        public string DisplayName => ItemProperties.Identified
            ? Info.Name
            : ItemProperties.UnidentifiedName;

        public override string ToString() => $"{DisplayName}";
    }
}
