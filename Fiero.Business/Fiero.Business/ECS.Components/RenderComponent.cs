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
        public ColorName? BorderColor { get; set; } = null;
        public string Label { get; set; } = null;
        public VisibilityName Visibility { get; set; } = VisibilityName.Visible;
        public bool Hidden { get; set; }
    }
}
