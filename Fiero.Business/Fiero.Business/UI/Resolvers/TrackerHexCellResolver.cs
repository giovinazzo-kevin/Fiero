using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    public class TrackerHexCellResolver : UIControlResolver<TrackerHexCell>
    {
        public TrackerHexCellResolver(
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

        public override TrackerHexCell Resolve(LayoutGrid dom)
        {
            var x = new TrackerHexCell(Input, GetText);
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            x.ContentAwareScale.V = false;
            x.FontSize.V = 8;
            return x;
        }
    }
}
