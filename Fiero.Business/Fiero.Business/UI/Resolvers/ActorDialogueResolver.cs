using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    public class ActorDialogueResolver : UIControlResolver<ActorDialogue>
    {
        public ActorDialogueResolver(GameInput input, GameDataStore store, GameFonts<FontName> fonts, GameSounds<SoundName> sounds, GameSprites<TextureName> sprites, GameLocalizations<LocaleName> localizations)
            : base(input, store, fonts, sounds, sprites, localizations)
        {
        }

        public override ActorDialogue Resolve(Coord position, Coord size)
        {
            var x = new ActorDialogue(Input, TileSize, GetSound, GetText, GetSprite);
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            x.Position.V = position;
            x.Size.V = size;
            x.ContentAwareScale.V = false;
            x.CenterContent.V = false;
            x.FontSize.V = 16;
            return x;
        }
    }
}
