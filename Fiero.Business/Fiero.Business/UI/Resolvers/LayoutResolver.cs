using Fiero.Core;

namespace Fiero.Business
{
    public class LayoutResolver : UIControlResolver<Layout>
    {
        public LayoutResolver(GameInput input, GameDataStore store, GameFonts<FontName> fonts, GameSounds<SoundName> sounds, GameSprites<TextureName> sprites, GameLocalizations<LocaleName> localizations)
            : base(input, store, fonts, sounds, sprites, localizations)
        {
        }

        public override Layout Resolve(Coord position, Coord size)
        {
            var x = new Layout(Input);
            x.Foreground.V = ActiveForeground;
            x.Background.V = ActiveBackground;
            x.Position.V = position;
            x.Size.V = size;
            return x;
        }
    }
}
