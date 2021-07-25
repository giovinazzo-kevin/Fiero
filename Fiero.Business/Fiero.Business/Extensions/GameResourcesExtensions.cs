using SFML.Graphics;

namespace Fiero.Business
{
    public static class GameResourcesExtensions
    {
        public static Sprite MakeSprite(this GameResources resources, RenderComponent component)
        {
            if(resources.Sprites.TryGet(component.TextureName, component.SpriteName, out var sprite, component.GetHashCode())) {
                if(resources.Colors.TryGet(component.Color, out var color)) {
                    sprite.Color = color;
                }
                return sprite;
            }
            return null;
        }
    }
}
