using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    public class ActorDialogueResolver : UIControlResolver<BetterActorDialogue>
    {
        public ActorDialogueResolver(GameUI ui, GameInput input, GameDataStore store, GameFonts<FontName> fonts, GameSounds<SoundName> sounds, GameSprites<TextureName> sprites, GameLocalizations<LocaleName> localizations)
            : base(ui, input, store, fonts, sounds, sprites, localizations)
        {
        }

        public override BetterActorDialogue Resolve(Coord position, Coord size)
        {
            var x = new BetterActorDialogue(Input, UI, GetSound);
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            x.Position.V = position;
            x.Size.V = size;
            return x;
        }
    }
}
