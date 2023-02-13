using Fiero.Core;

namespace Fiero.Business
{


    public class RenderComponent : EcsComponent
    {
        public RenderComponent()
        {
        }

        public string Sprite { get; set; } = "None";
        public RenderLayerName Layer { get; set; }
        public TextureName Texture { get; set; }
        public ColorName Color { get; set; } = ColorName.White;
        public VisibilityName Visibility { get; set; } = VisibilityName.Visible;
        public bool Hidden { get; set; }
    }
}
