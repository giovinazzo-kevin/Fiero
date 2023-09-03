using SFML.Graphics;

namespace Fiero.Core
{
    /// <summary>
    /// Wraps a UIWindow into a control, in order to create nested layouts.
    /// </summary>
    public class UIWindowAsControl : UIControl
    {
        public readonly UIControlProperty<UIWindow> Window = new(nameof(Window), null);

        public UIWindowAsControl(GameInput input) : base(input)
        {
            Size.ValueUpdated += (_, __) =>
            {
                if (Window.V?.Layout is null) return;
                Window.V.Layout.Size.V = Size.V;
            };
            Position.ValueUpdated += (_, __) =>
            {
                if (Window.V?.Layout is null) return;
                Window.V.Layout.Position.V = Position.V;
            };
            Window.ValueUpdated += (_, __) =>
            {
                if (Window.V?.Layout is null) return;
                Window.V.Layout.Position.V = Position.V;
                Window.V.Layout.Size.V = Size.V;
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
