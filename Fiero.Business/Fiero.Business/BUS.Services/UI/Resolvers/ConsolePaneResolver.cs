using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<ConsolePane>))]
    public class ConsolePaneResolver : UIControlResolver<ConsolePane>
    {
        public readonly KeyboardInputReader Reader;
        public ConsolePaneResolver(GameUI ui, GameResources resources, KeyboardInputReader reader)
            : base(ui, resources)
        {
            Reader = reader;
        }

        public override ConsolePane Resolve(LayoutGrid dom)
        {
            var x = new ConsolePane(UI.Input, Reader);
            x.Font.V = GetFont();
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            x.FontSize.V = x.Font.V.Size;
            return x;
        }
    }
}
