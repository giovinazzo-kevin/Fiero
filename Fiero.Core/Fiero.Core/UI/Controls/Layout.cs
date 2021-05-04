using SFML.Graphics;
using SFML.Window;
using System.Linq;

namespace Fiero.Core
{
    public class Layout : UIControl
    {
        protected UIControl ActiveControl { get; set; }

        public Layout(GameInput input, params UIControl[] controls) : base(input)
        {
            Children.AddRange(controls);
            ActiveControl = Children.FirstOrDefault();
            ActiveControl.IsActive = true;
        }

        public override void Update(float t, float dt)
        {
            var mousePos = Input.GetMousePosition().ToCoord();
            if (Input.IsButtonPressed(Mouse.Button.Left)) {
                var newActiveControl = default(UIControl);
                foreach (var child in Children) {
                    if(child.Contains(mousePos, out var clickedControl)) {
                        newActiveControl = clickedControl;
                        break;
                    }
                }
                if(newActiveControl != ActiveControl) {
                    if (ActiveControl != null) {
                        ActiveControl.IsActive = false;
                        ActiveControl = null;
                    }
                    ActiveControl = newActiveControl;
                    if(ActiveControl != null) {
                        ActiveControl.IsActive = true;
                        ActiveControl.Click(mousePos);
                    }
                }
            }
            ActiveControl?.Update(t, dt);
        }
    }
}
