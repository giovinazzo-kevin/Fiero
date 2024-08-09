using Fiero.Core;

namespace Fiero.Business
{
    [UIResolver<ComboBox>]
    public class ComboboxResolver : UIControlResolver<ComboBox>
    {
        public ComboboxResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }

        public override ComboBox Resolve()
        {
            var x = new ComboBox(UI.Input, () => new(UI.Input));
            x.Font.V = GetFont(FontName.Light);
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            x.Accent.V = Accent;
            return x;
        }
    }
}
