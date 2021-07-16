using Fiero.Core;
using SFML.Graphics;
using System.Drawing;

namespace Fiero.Business
{
    public abstract class View
    {
        public readonly GameWindow Window;

        public View(GameWindow window)
        {
            Window = window;
        }

        public abstract void OnWindowResized(Coord newSize);
        public virtual void Draw() { }
        public virtual void Update() { }
    }
}
