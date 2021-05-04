using Fiero.Core;
using SFML.Graphics;
using System.Drawing;

namespace Fiero.Business
{
    public abstract class View
    {
        public abstract void OnWindowResized(Coord newSize);
        public virtual void Draw(RenderWindow win, float t, float dt) { }
        public virtual void Update(RenderWindow win, float t, float dt) { }

    }
}
