using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{

    [TransientDependency]
    public class MiniMap : Widget
    {
        protected readonly DungeonSystem FloorSystem;
        protected readonly FactionSystem FactionSystem;
        protected readonly GameColors<ColorName> Colors;

        public readonly UIControlProperty<Actor> Following = new(nameof(Following), null);

        private RenderTexture _renderTexture;
        private Sprite _renderSprite;
        private bool _dirty = true;

        public MiniMap(
            GameUI ui,
            DungeonSystem floor,
            FactionSystem faction,
            GameColors<ColorName> colors
        ) : base(ui)
        {
            FloorSystem = floor;
            FactionSystem = faction;
            Colors = colors;
            Following.ValueChanged += (_, __) => SetDirty();
            Dragged += (_, __) => SetDirty();
            Dropped += (_, __) => SetDirty();
        }

        public override void Open(string title)
        {
            base.Open(title);
            Size.ValueChanged += (_, __) =>
            {
                _renderTexture?.Dispose();
                _renderSprite?.Dispose();
                SetDirty();
            };
            Layout.Invalidated += (_) => SetDirty();
            SetDirty();
        }

        protected void Repaint()
        {
            if (Following.V == null)
                return;
            var floorId = Following.V.FloorId();
            if (!FloorSystem.TryGetFloor(floorId, out var floor))
                return;
            _renderTexture = new((uint)floor.Size.X, (uint)floor.Size.Y) { Smooth = false };
            _renderSprite = new(_renderTexture.Texture);
        }

        public void SetDirty() => _dirty = true;

        public override void Draw()
        {
            base.Draw();
            if (_dirty && Following.V != null)
            {
                Repaint();
                if (!Bake())
                    return;
            }
            UI.Window.Draw(_renderSprite);
            bool Bake()
            {
                var floorId = Following.V.FloorId();
                if (!FloorSystem.TryGetFloor(floorId, out var floor))
                    return false;
                _renderTexture.Clear(Color.Transparent);
                using var whitePixel = new RenderTexture(1, 1);
                whitePixel.Clear(Color.White);
                whitePixel.Display();
                foreach (var coord in floor.Size.ToRect().Enumerate())
                {
                    if (!floor.Cells.TryGetValue(coord, out var cell))
                        continue;

                    var allseeing = Following.V.Fov.Sight.HasFlag(VisibilityName.TrueSight);
                    var known = allseeing || Following.V.Fov.KnownTiles.TryGetValue(floorId, out var coords) && coords.Contains(coord);
                    var seen = Following.V.Fov.VisibleTiles.TryGetValue(floorId, out coords) && coords.Contains(coord);
                    if (!known)
                        continue;
                    if (
                           coord.X < 0 || coord.X >= Layout.Size.V.X
                        || coord.Y < 0 || coord.Y >= Layout.Size.V.Y)
                    {
                        continue;
                    }

                    foreach (var drawable in cell.GetDrawables(Following.V.Fov.Sight, seen))
                    {
                        if (drawable.Render.Hidden)
                            continue;
                        using var sprite = new Sprite(whitePixel.Texture);
                        sprite.Color = Colors.Get(drawable switch
                        {
                            Feature x when x.FeatureProperties.Name == FeatureName.Door && x.Physics.BlocksLight => ColorName.White,
                            Tile x when x.TileProperties.Name == TileName.Corridor && seen => ColorName.LightMagenta,
                            Tile x when x.TileProperties.Name == TileName.Corridor => ColorName.Magenta,
                            Tile x when x.Physics.BlocksMovement => ColorName.White,
                            Tile x when !x.Physics.BlocksMovement && seen => ColorName.LightBlue,
                            Tile x when !x.Physics.BlocksMovement => ColorName.Blue,
                            Item x => ColorName.LightCyan,
                            Feature x when x.FeatureProperties.Name == FeatureName.Trap => ColorName.Black,
                            Feature x when x.FeatureProperties.Name == FeatureName.Downstairs => ColorName.LightGreen,
                            Feature x when x.FeatureProperties.Name == FeatureName.Upstairs => ColorName.Green,
                            Actor x when x == Following.V => ColorName.White,
                            Actor x when FactionSystem.GetRelations(x, Following).Left.IsFriendly() => ColorName.LightYellow,
                            Actor x when FactionSystem.GetRelations(x, Following).Left.IsHostile() => ColorName.LightRed,
                            Actor x => ColorName.LightGray,
                            PhysicalEntity x when x.Physics.BlocksMovement => ColorName.Gray,
                            _ => ColorName.Black
                        });
                        sprite.Position = coord + Coord.PositiveOne;
                        var spriteSize = sprite.GetLocalBounds().Size();
                        sprite.Origin = new Vec(0.5f, 0.5f) * spriteSize;
                        _renderTexture.Draw(sprite);
                    }
                }
                _renderTexture.Display();
                _renderSprite.Position = Layout.Position.V;
                // The scale division is not done with floating point numbers because the resulting minimap
                // has unevenly-sized pixels. However, since at this scale the minimap won't use all available
                // space, we also need to re-center it by half the difference between its scaled and original size
                _renderSprite.Scale = Layout.Size.V / floor.Size;
                var delta = _renderSprite.TextureRect.Size() * _renderSprite.Scale.ToVec() - _renderSprite.TextureRect.Size();
                _renderSprite.Position += (delta / 2).Abs().ToCoord();
                _dirty = false;
                return true;
            }
        }
    }
}
