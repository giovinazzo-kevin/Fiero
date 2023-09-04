using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{

    [TransientDependency]
    public class MiniMap : Widget
    {
        public readonly DungeonSystem FloorSystem;
        public readonly FactionSystem FactionSystem;
        public readonly GameColors<ColorName> Colors;

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
            Layout.Size.ValueChanged += (_, __) =>
            {
                _renderTexture?.Dispose();
                _renderSprite?.Dispose();
                SetDirty();
            };
            Layout.Position.ValueChanged += (_, __) => SetDirty();
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
        protected override void DefaultSize() { }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            base.Draw(target, states);
            if (_dirty && Following.V != null)
            {
                Repaint();
                if (!Bake())
                    return;
            }
            target.Draw(_renderSprite);
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
                            Feature x when x.FeatureProperties.Name == FeatureName.Door && x.Physics.BlocksLight => ColorName.Yellow,
                            Tile x when x.TileProperties.Name == TileName.Corridor && seen => ColorName.LightMagenta,
                            Tile x when x.TileProperties.Name == TileName.Corridor => ColorName.Magenta,
                            Tile x when x.Physics.BlocksMovement => ColorName.White,
                            Tile x when !x.Physics.BlocksMovement && seen => ColorName.LightBlue,
                            Tile x when !x.Physics.BlocksMovement => ColorName.Blue,
                            Item x => ColorName.LightCyan,
                            Feature x when x.FeatureProperties.Name == FeatureName.Trap => ColorName.Black,
                            Feature x when x.FeatureProperties.Name == FeatureName.Downstairs => ColorName.LightGreen,
                            Feature x when x.FeatureProperties.Name == FeatureName.Upstairs => ColorName.Green,
                            Actor x when x == Following.V => ColorName.Cyan,
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
                _renderSprite.Scale = Layout.Size.V / floor.Size; // int division to preserve AR
                var delta = _renderSprite.GetLocalBounds().Size() * _renderSprite.Scale.ToVec() / 2 - Layout.Size.V / 2;
                _renderSprite.Position -= delta;
                _dirty = false;
                return true;
            }
        }
    }
}
