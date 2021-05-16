using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    public class ActorDialogueResolver : UIControlResolver<ActorDialogue>
    {
        public ActorDialogueResolver(
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

        public override ActorDialogue Resolve(LayoutGrid dom)
        {
            var x = new ActorDialogue(dom, Input, Store, UI, GetSound, GetColor);
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            return x;
        }
    }
}
