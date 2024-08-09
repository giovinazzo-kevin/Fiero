namespace Fiero.Business
{


    public class RenderComponent : EcsComponent
    {
        public RenderComponent()
        {
        }

        public int Layer { get; set; }
        public string Sprite { get; set; } = "None";
        public string Texture { get; set; }
        public string Color { get; set; } = ColorName.White;
        public string BorderColor { get; set; } = null;
        public string Label { get; set; } = null;
        public VisibilityName Visibility { get; set; } = VisibilityName.Visible;
        public bool Hidden { get; set; }
    }
}
