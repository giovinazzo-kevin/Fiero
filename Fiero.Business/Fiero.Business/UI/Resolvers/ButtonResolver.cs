using Fiero.Core;

namespace Fiero.Business
{
    public class ButtonResolver : UIControlResolver<Button>
    {
        public ButtonResolver(GameUI ui, GameInput input, GameDataStore store, GameFonts<FontName> fonts, GameSounds<SoundName> sounds, GameSprites<TextureName> sprites, GameLocalizations<LocaleName> localizations)
            : base(ui, input, store, fonts, sounds, sprites, localizations)
        {
        }

        public override Button Resolve(Coord position, Coord size)
        {
            var x = new Button(Input, GetText);
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            x.Position.V = position;
            x.Size.V = size;
            x.ContentAwareScale.V = false;
            x.FontSize.V = 24;
            return x;
        }
    }
}
