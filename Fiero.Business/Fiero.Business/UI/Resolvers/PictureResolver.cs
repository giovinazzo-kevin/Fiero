using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    public class PictureResolver : UIControlResolver<Picture<TextureName>>
    {
        public PictureResolver(GameUI ui, GameInput input, GameDataStore store, GameFonts<FontName> fonts, GameSounds<SoundName> sounds, GameSprites<TextureName> sprites, GameLocalizations<LocaleName> localizations)
            : base(ui, input, store, fonts, sounds, sprites, localizations)
        {
        }

        public override Picture<TextureName> Resolve(Coord position, Coord size)
        {
            var x = new Picture<TextureName>(Input, GetSprite);
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            x.Position.V = position;
            x.Size.V = size;
            return x;
        }
    }
}
