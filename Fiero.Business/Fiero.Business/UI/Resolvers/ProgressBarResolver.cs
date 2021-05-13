using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    public class ProgressBarResolver : UIControlResolver<ProgressBar>
    {
        public ProgressBarResolver(GameInput input, GameDataStore store, GameFonts<FontName> fonts, GameSounds<SoundName> sounds, GameSprites<TextureName> sprites, GameLocalizations<LocaleName> localizations)
            : base(input, store, fonts, sounds, sprites, localizations)
        {
        }
        public override ProgressBar Resolve(Coord position, Coord size)
        {
            var x = new ProgressBar(Input, TileSize,
                GetSprite("bar_empty-l"), GetSprite("bar_empty-m"), GetSprite("bar_empty-r"),
                GetSprite("bar_half-l"), GetSprite("bar_half-m"), GetSprite("bar_half-r"),
                GetSprite("bar_full-l"), GetSprite("bar_full-m"), GetSprite("bar_full-r"));
            x.Foreground.V = ActiveForeground;
            x.Background.V = ActiveBackground;
            x.Position.V = position;
            x.Size.V = size;
            return x;
        }
    }
}
