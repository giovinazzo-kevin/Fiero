using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<Picture<TextureName>>))]
    public class PictureResolver : UIControlResolver<Picture<TextureName>>
    {
        public PictureResolver(
            GameUI ui, 
            GameInput input, 
            GameDataStore store, 
            GameFonts<FontName> fonts, 
            GameSounds<SoundName> sounds, 
            GameColors<ColorName> colors,
            GameSprites<TextureName> sprites, 
            GameLocalizations<LocaleName> localizations)
            : base(ui, input, store, fonts, sounds, colors, sprites, localizations)
        {
        }

        public override Picture<TextureName> Resolve(LayoutGrid dom)
        {
            var x = new Picture<TextureName>(Input, GetSprite);
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            return x;
        }
    }
}
