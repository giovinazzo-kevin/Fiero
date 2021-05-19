using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    public class TrackerNoteCellResolver : UIControlResolver<TrackerNoteCell>
    {
        public TrackerNoteCellResolver(
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

        public override TrackerNoteCell Resolve(LayoutGrid dom)
        {
            var x = new TrackerNoteCell(Input, GetText);
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            x.ContentAwareScale.V = false;
            x.FontSize.V = 8;
            return x;
        }
    }
}
