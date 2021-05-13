using Fiero.Core;
using System;

namespace Fiero.Business
{
    public class LabelResolver : UIControlResolver<Label>
    {
        public LabelResolver(GameInput input, GameDataStore store, GameFonts<FontName> fonts, GameSounds<SoundName> sounds, GameSprites<TextureName> sprites, GameLocalizations<LocaleName> localizations)
            : base(input, store, fonts, sounds, sprites, localizations)
        {
        }

        public override Label Resolve(Coord position, Coord size)
        {
            var x = new Label(Input, GetText);
            x.Foreground.V = ActiveForeground;
            x.Background.V = ActiveBackground;
            x.Position.V = position;
            x.Size.V = size;
            x.ContentAwareScale.V = false;
            x.FontSize.V = 24;
            return x;
        }
    }
}
