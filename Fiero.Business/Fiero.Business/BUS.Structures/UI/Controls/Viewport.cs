using Ergo.Lang;
using SFML.Graphics;

namespace Fiero.Business
{

    /// <summary>
    /// Represents a drawable view that encompasses all tiles, items, features and actors within the specified bounds.
    /// </summary>
    public class Viewport : UIControl
    {
        protected readonly DungeonSystem FloorSystem;
        protected readonly FactionSystem FactionSystem;
        protected readonly GameResources Resources;
        protected readonly GameLoop Loop;

        public readonly UIControlProperty<IntRect> ViewArea = new(nameof(ViewArea), new(0, 0, 40, 40));
        public readonly UIControlProperty<bool> AutoUpdateViewArea = new(nameof(AutoUpdateViewArea), true);
        public readonly UIControlProperty<Coord> ViewTileSize = new(nameof(ViewTileSize), new(32, 32));
        public readonly UIControlProperty<TargetingShape> TargetingShape = new(nameof(TargetingShape), default);
        public readonly UIControlProperty<Actor> Following = new(nameof(Following), null);

        private Coord _cachedPos;
        private RenderTexture _renderTexture;
        private Sprite _renderSprite;
        private bool _dirty = true;

        public Viewport(
            GameInput input,
            DungeonSystem floor,
            FactionSystem faction,
            GameResources res,
            GameLoop loop
        ) : base(input)
        {
            FloorSystem = floor;
            FactionSystem = faction;
            Resources = res;
            Loop = loop;
            Size.ValueChanged += (_, __) =>
            {
                _renderTexture?.Dispose();
                _renderSprite?.Dispose();
                _renderTexture = new((uint)Size.V.X, (uint)Size.V.Y) { Smooth = false };
                _renderSprite = new(_renderTexture.Texture);
                if (AutoUpdateViewArea.V)
                {
                    var viewPos = ViewArea.V.Position();
                    ViewArea.V = new(
                        viewPos.X,
                        viewPos.Y,
                        Size.V.X / ViewTileSize.V.X,
                        Size.V.Y / ViewTileSize.V.Y
                    );
                }
                SetDirty();
            };
            ViewArea.ValueChanged += (_, __) => SetDirty();
            Following.ValueChanged += (_, __) => SetDirty();
            ViewTileSize.ValueChanged += (_, __) => SetDirty();
            TargetingShape.ValueChanged += (_, old) =>
            {
                if (old != null)
                    old.Changed -= OnChanged;
                if (TargetingShape.V != null)
                {
                    TargetingShape.V.Changed += OnChanged;
                    OnChanged(TargetingShape.V);
                }

                void OnChanged(TargetingShape v)
                {
                    Invalidate();
                }
            };
            Invalidated += (_) => SetDirty();
        }

        public void SetDirty() => _dirty = true;

        /// <summary>
        /// Returns the position of the tile currently highlighted by the mouse, or none if the mouse is not over the viewport.
        /// </summary>
        public Maybe<Coord> MouseToWorldPos()
        {
            if (!IsMouseOver)
                return default;
            var pos = TrackedMousePosition - Position.V;
            var worldPos = pos / ViewTileSize.V + _cachedPos;
            return worldPos;
        }

        public Coord ScreenToWorldPos(Coord screen)
        {
            var pos = screen - Position.V;
            var worldPos = pos / ViewTileSize.V + Following.V.Position();
            return worldPos;
        }

        public Coord WorldToScreenPos(Coord world)
        {
            return (world - Following.V.Position() - Coord.PositiveOne)
                * ViewTileSize.V + (Position.V + Size.V / 2).Align(ViewTileSize) + ViewTileSize;
        }


        protected override void Repaint(RenderTarget target, RenderStates states)
        {
            base.Repaint(target, states);
            if (Following.V is null)
                return;
            if (_dirty)
            {
                if (!Bake())
                    return;
            }
            if (TargetingShape.V is { } shape)
            {
                using var darkerSprite = new Sprite(_renderSprite);
                darkerSprite.Color = new(128, 128, 128);
                target.Draw(darkerSprite);

                DrawTargetingShape(shape);
            }
            else
            {
                target.Draw(_renderSprite);
            }


            void DrawTargetingShape(TargetingShape shape)
            {
                foreach (var point in shape.GetPoints())
                {
                    var pos = (point - new Coord(ViewArea.V.Left, ViewArea.V.Top)) * ViewTileSize.V + Position.V;
                    var origin = new Vec(0.5f, 0.5f) * ViewTileSize.V;
                    var spriteRect = new IntRect(pos.X - (int)origin.X, pos.Y - (int)origin.Y, ViewTileSize.V.X, ViewTileSize.V.Y);
                    using var sprite = new Sprite(_renderSprite.Texture, spriteRect)
                    {
                        Position = pos,
                        Origin = origin
                    };
                    using var highlight = new RectangleShape(ViewTileSize.V)
                    {
                        Position = pos,
                        Origin = origin,
                        FillColor = new(0, 0, 0, 0),
                        OutlineColor = new(255, 255, 0, 255),
                        OutlineThickness = 1
                    };
                    target.Draw(sprite);
                    target.Draw(highlight);
                }
            }

            bool Bake()
            {
                var layers = new Dictionary<RenderLayerName, Action<RenderTexture>>();
                foreach (var key in Enum.GetValues<RenderLayerName>())
                {
                    layers[key] = _ => { };
                }
                var floorId = Following.V.FloorId();
                if (!FloorSystem.TryGetFloor(floorId, out var floor))
                    return false;
                _renderTexture.Clear(Background.V);
                var screenBounds = Position.V + Size.V;
                var area = new IntRect(ViewArea.V.Left, ViewArea.V.Top, ViewArea.V.Width + 1, ViewArea.V.Height + 1);
                foreach (var coord in area.Enumerate())
                {
                    if (!floor.Cells.TryGetValue(coord, out var cell))
                        continue;

                    var known = Following.V.Fov.KnownTiles[floorId].Contains(coord);
                    var seen = Following.V.Fov.VisibleTiles[floorId].Contains(coord);

                    if (!known)
                        continue;

                    var relativePos = coord - new Coord(ViewArea.V.Left, ViewArea.V.Top);
                    var screenPos = relativePos * ViewTileSize.V + Position.V;
                    if (
                           screenPos.X < -ViewTileSize.V.X || screenPos.X >= screenBounds.X + ViewTileSize.V.X
                        || screenPos.Y < -ViewTileSize.V.Y || screenPos.Y >= screenBounds.Y + ViewTileSize.V.Y)
                    {
                        continue;
                    }

                    foreach (var drawable in cell.GetDrawables(Following.V.Fov.Sight, seen))
                    {
                        if (drawable.Render.Hidden)
                            continue;
                        var asActor = drawable as Actor;
                        var rngSeed = drawable.Render.GetHashCode();
                        // Draw allegiance circle
                        if (asActor != null && asActor.Faction.Name != FactionName.None)
                        {
                            layers[RenderLayerName.BackgroundEffects] += tex =>
                            {
                                var color = FactionSystem.GetRelations(Following.V, asActor).Left switch
                                {
                                    var _ when asActor.IsPlayer() => ColorName.LightCyan,
                                    var x when x.IsHostile() => ColorName.LightRed,
                                    var x when x.IsFriendly() => ColorName.LightGreen,
                                    _ => ColorName.Yellow
                                };
                                if (!Resources.Sprites.TryGet(TextureName.Icons, "AllegianceCircle", color, out var circleDef, rngSeed))
                                    return;
                                using var sprite = new Sprite(circleDef);
                                var spriteSize = sprite.GetLocalBounds().Size();
                                sprite.Origin = new Vec(0.5f, 0.5f) * spriteSize;
                                sprite.Scale = ViewTileSize.V / spriteSize;
                                sprite.Position = screenPos;
                                tex.Draw(sprite, states);
                            };
                        }
                        // Draw sprite
                        layers[drawable.Render.Layer] += tex =>
                        {
                            var spriteName = drawable.Render.Sprite;
                            var borderColor = drawable.Render.BorderColor;
                            var label = drawable.Render.Label;
                            if (drawable.TryCast<Item>(out var item))
                            {
                                spriteName = item.ItemProperties.ItemSprite ?? spriteName;
                            }
                            if (Resources.Sprites.TryGet(drawable.Render.Texture, spriteName, drawable.Render.Color, out var spriteDef, rngSeed))
                            {
                                using var sprite = new Sprite(spriteDef);
                                var spriteSize = sprite.GetLocalBounds().Size();
                                sprite.Origin = new Vec(0.5f, 0.5f) * spriteSize;
                                sprite.Scale = ViewTileSize.V / spriteSize;
                                sprite.Position = screenPos;
                                if (asActor != null && asActor.Faction.Name != FactionName.None)
                                    sprite.Position -= new Vec(0f, 0.33f) * spriteSize;
                                if (!seen)
                                {
                                    sprite.Color = sprite.Color.AddRgb(-64, -64, -64);
                                }
                                tex.Draw(sprite, states);
                                if (borderColor != null)
                                {
                                    using var highlight = new RectangleShape(ViewTileSize.V - Coord.PositiveOne * 2)
                                    {
                                        Position = sprite.Position + Coord.PositiveOne,
                                        Origin = sprite.Origin.ToVec() * sprite.Scale.ToVec(),
                                        FillColor = new(0, 0, 0, 0),
                                        OutlineColor = Resources.Colors.Get(borderColor.Value),
                                        OutlineThickness = 1
                                    };
                                    tex.Draw(highlight, states);
                                }
                                if (label != null)
                                {
                                    var labelColor = borderColor ?? drawable.Render.Color;
                                    var font = Resources.Fonts.Get(FontName.Monospace);
                                    var text = new BitmapText(font, label)
                                    {
                                        Position = sprite.Position.ToCoord() - ViewTileSize.V / 2
                                            - new Coord(0, font.Size.Y + 1),
                                        FillColor = Resources.Colors.Get(labelColor)
                                    };
                                    tex.Draw(text, states);
                                }
                            }
                        };
                        // Draw active effects
                        if (asActor != null && asActor.Effects != null)
                        {
                            var offs = Coord.Zero;
                            int _i = 0;
                            foreach (var effect in asActor.Effects.Active)
                            {
                                var icon = effect.Name.ToString();
                                layers[RenderLayerName.ForegroundEffects] += tex =>
                                {
                                    if (Resources.Sprites.TryGet(TextureName.Icons, icon, ColorName.White, out var iconDef, rngSeed))
                                    {
                                        using var iconSprite = new Sprite(iconDef);
                                        var iconSize = iconSprite.GetLocalBounds().Size();
                                        var scale = (iconSprite.Scale = ViewTileSize.V / iconSize / 4).ToCoord();
                                        iconSprite.Position = screenPos + offs - iconSize * scale;
                                        iconSprite.Origin = new Vec(1f, 1f) * iconSize;
                                        if (_i++ % 4 == 3)
                                        {
                                            offs += iconSize.ToCoord() * scale * new Coord(0, 1);
                                            offs *= new Coord(0, 1);
                                        }
                                        else
                                        {
                                            offs += iconSize.ToCoord() * scale * new Coord(1, 0);
                                        }
                                        tex.Draw(iconSprite, states);
                                    }
                                };

                            }
                        }
                    }
                }
                foreach (var key in Enum.GetValues<RenderLayerName>())
                {
                    layers[key](_renderTexture);
                }
                _renderTexture.Display();
                _dirty = false;
                return true;
            }
        }
    }
}
