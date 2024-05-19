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
        protected readonly GameDataStore Store;

        public UIControlProperty<IntRect> ViewArea {get; private set;} = new(nameof(ViewArea), new(0, 0, 40, 40));
        public UIControlProperty<bool> AutoUpdateViewArea {get; private set;} = new(nameof(AutoUpdateViewArea), true);
        public UIControlProperty<Coord> ViewTileSize {get; private set;} = new(nameof(ViewTileSize), new(16, 16));
        public UIControlProperty<TargetingShape> TargetingShape {get; private set;} = new(nameof(TargetingShape), default);
        public UIControlProperty<Actor> Following {get; private set;} = new(nameof(Following), null);

        private Coord _cachedPos;
        private RenderTexture _renderTexture;
        private Sprite _renderSprite;
        private bool _dirty = true;

        public void UpdateViewArea()
        {
            var viewPos = ViewArea.V.Position();
            ViewArea.V = new(
                viewPos.X,
                viewPos.Y,
                Size.V.X / ViewTileSize.V.X,
                Size.V.Y / ViewTileSize.V.Y
            );
        }

        public override void Update(TimeSpan t, TimeSpan dt)
        {
            base.Update(t, dt);
            if (MouseToWorldPos().TryGetValue(out var mousePos)
                && ToolTip.V is CellToolTip tooltip)
            {
                var floorId = Following.V.FloorId();
                if (Following.V.Knows(floorId, mousePos)
                    && FloorSystem.TryGetCellAt(floorId, mousePos, out var cell))
                    tooltip.Cell.V = cell;
                else
                    tooltip.Cell.V = null;
            }
        }

        public Viewport(
            GameInput input,
            DungeonSystem floor,
            FactionSystem faction,
            GameResources res,
            GameLoop loop,
            GameDataStore store
        ) : base(input)
        {
            Store = store;
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
                    UpdateViewArea();
                }
                SetDirty();
            };
            ViewArea.ValueChanged += (_, __) => SetDirty();
            Following.ValueChanged += (_, __) => SetDirty();
            ViewTileSize.ValueChanged += (_, __) =>
            {
                UpdateViewArea();
                SetDirty();
            };
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
            var worldPos = (pos + ViewTileSize.V / 2) / ViewTileSize.V + _cachedPos + ViewArea.V.Position();
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
                var tileSize = Store.Get(Data.View.TileSize);
                var zoom = ViewTileSize.V.X / tileSize;

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
                                if (!drawable.TryCast<Tile>(out _))
                                {
                                    sprite.Scale = ViewTileSize.V / spriteSize;
                                    sprite.Origin = new Vec(0.5f, 0.5f) * spriteSize;
                                }
                                else
                                {
                                    var tileSize = Store.Get(Data.View.TileSize);
                                    sprite.Scale = ViewTileSize.V / tileSize;
                                    // Some tiles are taller than 16px, offset them accordingly
                                    sprite.Origin = new Vec(0.5f, 0.5f) * tileSize + (spriteSize - tileSize);
                                }
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
                        if (asActor != null)
                        {
                            // Draw active effects
                            if (asActor.Effects != null)
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
                            // Draw HP/MP bars
                            var hpPct = asActor.ActorProperties.Health?.Percentage ?? 0;
                            var mpPct = asActor.ActorProperties.Magic?.Percentage ?? 0;
                            layers[RenderLayerName.ForegroundEffects] += tex =>
                            {
                                if (hpPct > 0 && hpPct < 1)
                                {
                                    using var hpRect = new RectangleShape()
                                    {
                                        Position = screenPos - ViewTileSize.V / 2 + new Coord(zoom, -ViewTileSize.V.X / 4 - zoom - 1),
                                        FillColor = Resources.Colors.Get(ColorName.LightRed),
                                        Size = new((ViewTileSize.V.X - zoom * 2) * hpPct, zoom)
                                    };
                                    tex.Draw(hpRect, states);
                                }
                                if (mpPct > 0 && mpPct < 1)
                                {
                                    using var mpRect = new RectangleShape()
                                    {
                                        Position = screenPos - ViewTileSize.V / 2 + new Coord(zoom, -ViewTileSize.V.X / 4),
                                        FillColor = Resources.Colors.Get(ColorName.LightBlue),
                                        Size = new((ViewTileSize.V.X - zoom * 2) * mpPct, zoom)
                                    };
                                    tex.Draw(mpRect, states);
                                }
                            };
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
