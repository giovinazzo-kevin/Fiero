using Fiero.Core;
using SFML.Graphics;
using System.Drawing;

namespace Fiero.Business
{
    public abstract class View
    {
        public View()
        {

        }

        public abstract void OnWindowResized(Coord newSize);
        public virtual void Draw(RenderTarget target) { }
        public virtual void Update() { }
    }
}
