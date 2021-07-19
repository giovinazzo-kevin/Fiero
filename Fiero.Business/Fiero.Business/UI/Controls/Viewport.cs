using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    /// <summary>
    /// Represents a drawable view that encompasses all tiles, items, features and actors within the specified bounds.
    /// </summary>
    public class Viewport : UIControl
    {
        protected readonly FloorSystem FloorSystem;

        public readonly UIControlProperty<FloorId> ViewFloor = new(nameof(ViewFloor), new());
        public readonly UIControlProperty<IntRect> ViewArea = new(nameof(ViewArea), new(0, 0, 40, 40));
        public readonly UIControlProperty<Coord> ViewTileSize = new(nameof(ViewTileSize), new(16, 16));

        public Viewport(GameInput input, FloorSystem floor)
            : base(input)
        {
            FloorSystem = floor;
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            base.Draw(target, states);
            if (!FloorSystem.TryGetFloor(ViewFloor.V, out var floor))
                return;
            var screenBounds = Position.V + Size.V;
            foreach (var coord in ViewArea.V.Enumerate()) {
                if (!floor.Cells.TryGetValue(coord, out var cell))
                    continue;

                var relativePos = coord - new Coord(ViewArea.V.Left, ViewArea.V.Top);
                var screenPos = relativePos * ViewTileSize.V + Position.V;
                if (
                       screenPos.X < 0 || screenPos.X >= screenBounds.X
                    || screenPos.Y < 0 || screenPos.Y >= screenBounds.Y) {
                    continue;
                }

                foreach (var drawable in cell.GetDrawables()) {
                    using var sprite = new Sprite(drawable.Render.Sprite);
                    sprite.Position = screenPos;
                    sprite.Scale = ViewTileSize.V / sprite.GetLocalBounds().Size();
                    sprite.Draw(target, states);
                }
            }
        }
    }
}
