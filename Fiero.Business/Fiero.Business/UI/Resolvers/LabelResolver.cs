using Fiero.Core;
using SFML.Graphics;
using System;

namespace Fiero.Business
{
    public class LabelResolver : UIControlResolver<Label>
    {
        public LabelResolver(GameUI ui, GameInput input, GameDataStore store, GameFonts<FontName> fonts, GameSounds<SoundName> sounds, GameSprites<TextureName> sprites, GameLocalizations<LocaleName> localizations)
            : base(ui, input, store, fonts, sounds, sprites, localizations)
        {
        }

        public override Label Resolve(Coord position, Coord size)
        {
            var x = new Label(Input, GetText);
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            x.Position.V = position;
            x.Size.V = size;
            x.ContentAwareScale.V = false;
            x.FontSize.V = 24;
            return x;
        }
    }
}
