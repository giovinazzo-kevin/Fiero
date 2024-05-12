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
        private bool _refresh = false;

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
            //Layout.Invalidated += (_) => SetDirty();
            SetDirty();
        }

        protected void Repaint()
        {
            if (Following.V == null)
                return;
            var floorId = Following.V.FloorId();
            if (!FloorSystem.TryGetFloor(floorId, out var floor))
                return;
            var renderSize = new Coord(floor.Size.X + floor.Size.X % 2, floor.Size.Y + floor.Size.Y % 2);
            if (_renderTexture is null || _renderTexture.Size.ToCoord() != renderSize)
            {
                _renderTexture?.Dispose();
                _renderSprite?.Dispose();
                _renderTexture = new((uint)(floor.Size.X + floor.Size.X % 2), (uint)(floor.Size.Y + floor.Size.Y % 2)) { Smooth = false };
                _renderTexture.Clear(Color.Transparent);
                _renderSprite = new(_renderTexture.Texture);
            }
        }

        public void SetDirty()
        {
            _dirty = true;
            Layout?.Invalidate();
        }
        public void Refresh()
        {
            _refresh = true;
            SetDirty();
        }
        protected override void DefaultSize() { }
        private HashSet<Coord> lastFov = [];
        public override void Draw(RenderTarget target, RenderStates states)
        {
            base.Draw(target, states);
            if (_dirty && Following.V != null)
            {
                Repaint();
                if (!Bake())
                    return;
            }
            if (_renderSprite is null)
                return;
            target.Draw(_renderSprite);
            bool Bake()
            {
                var floorId = Following.V.FloorId();
                if (!FloorSystem.TryGetFloor(floorId, out var floor))
                    return false;
                if (!Following.V.Fov.VisibleTiles.TryGetValue(floorId, out var visibleCoords))
                    return false;
                if (!Following.V.Fov.KnownTiles.TryGetValue(floorId, out var knownCoords))
                    return false;
                using var whitePixel = new RenderTexture(1, 1);
                whitePixel.Clear(Color.White);
                whitePixel.Display();
                foreach (var (coord, cell) in floor.Cells)
                {
                    var seen = visibleCoords.Contains(coord);
                    if (!seen && (!lastFov.Contains(coord) && !_refresh))
                        continue;
                    var known = knownCoords.Contains(coord);
                    if (!known)
                        continue;
                    if (
                           coord.X < 0 || coord.X >= Layout.Size.V.X
                        || coord.Y < 0 || coord.Y >= Layout.Size.V.Y)
                    {
                        continue;
                    }

                    foreach (var drawable in cell.GetDrawables(Following.V.Fov.Sight, seen)
                        .OrderByDescending(x => x.Render.Layer))
                    {
                        if (drawable.Render.Hidden)
                            continue;
                        var col = drawable switch
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
                            _ => ColorName.Transparent
                        };
                        if (col == ColorName.Transparent)
                            continue;
                        using var sprite = new Sprite(whitePixel.Texture);
                        sprite.Color = Colors.Get(col);
                        sprite.Position = coord + Coord.PositiveOne;
                        var spriteSize = sprite.GetLocalBounds().Size();
                        sprite.Origin = new Vec(0.5f, 0.5f) * spriteSize;
                        _renderTexture.Draw(sprite);
                        break;
                    }
                }
                _renderTexture.Display();
                _renderSprite.Position = Layout.Position.V;
                _renderSprite.Scale = Layout.Size.V / floor.Size; // int division to preserve AR
                var delta = _renderSprite.GetLocalBounds().Size() * _renderSprite.Scale.ToVec() / 2 - Layout.Size.V / 2;
                _renderSprite.Position -= delta;
                lastFov.Clear();
                lastFov.UnionWith(visibleCoords);
                _dirty = false;
                _refresh = false;
                return true;
            }
        }
    }
}
