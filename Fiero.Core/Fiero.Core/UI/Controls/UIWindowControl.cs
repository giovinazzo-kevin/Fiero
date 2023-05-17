using SFML.Graphics;

namespace Fiero.Core
{
    /// <summary>
    /// Encapsulate a UIWindow into a control, in order to create nested layouts.
    /// </summary>
    public class UIWindowControl : UIControl
    {
        public readonly UIControlProperty<UIWindow> Window = new(nameof(Window), null);

        public UIWindowControl(GameInput input) : base(input)
        {
            Size.ValueUpdated += (_, __) =>
            {
                if (Window.V is null) return;
                Window.V.Size.V = Size.V;
            };
            Position.ValueUpdated += (_, __) =>
            {
                if (Window.V is null) return;
                Window.V.Position.V = Position.V;
            };
            Window.ValueUpdated += (_, __) =>
            {
                if (Window is null) return;
                Window.V.Position.V = Position.V;
                Window.V.Size.V = Size.V;
            };
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            if (Window.V is null || !Window.V.IsOpen) return;
            Window.V.Draw();
        }

        public override void Update()
        {
            if (Window.V is null) return;
            Window.V.Update();
        }
    }
}
