using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<TextBox>))]
    public class TextboxResolver : UIControlResolver<TextBox>
    {
        public readonly KeyboardInputReader Reader;

        public TextboxResolver(GameUI ui, GameResources resources, KeyboardInputReader reader)
            : base(ui, resources)
        {
            Reader = reader;
        }

        public override TextBox Resolve(LayoutGrid dom)
        {
            var x = new TextBox(UI.Input, Reader);
            x.Font.V = GetFont();
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            x.ContentAwareScale.V = false;
            x.FontSize.V = x.Font.V.Size;
            return x;
        }
    }
}
