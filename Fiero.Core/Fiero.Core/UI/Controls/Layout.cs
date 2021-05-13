using SFML.Graphics;
using SFML.Window;
using System.Linq;

namespace Fiero.Core
{
    public class Layout : UIControl
    {
        public Layout(GameInput input, params UIControl[] controls) : base(input)
        {
            Children.AddRange(controls);
        }
    }
}
