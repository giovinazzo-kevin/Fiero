using Fiero.Core;
using Fiero.Core.Structures;
using SFML.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        private RenderTexture _renderTexture;
        private Sprite _renderSprite;
        private bool _dirty = true;

        protected readonly ConcurrentDictionary<int, ConcurrentQueue<OrderedPair<Coord, SpriteDef>>> Vfx = new();

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
        }

        public void SetDirty() => _dirty = true;

        public override void Draw(RenderTarget target, RenderStates states)
        {
            base.Draw(target, states);
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
            DrawVFX();

            void DrawVFX()
            {
                var viewPos = ViewArea.V.Position();
                foreach (var k in Vfx.Keys)
                {
                    if (!Vfx.TryGetValue(k, out var anim))
                    {
                        continue;
                    }
                    for (int j = 0, animCount = anim.Count; j < animCount && anim.TryDequeue(out var pair); j++)
                    {
                        var (worldPos, spriteDef) = (pair.Left, pair.Right);
                        using var sprite = new Sprite(Resources.Sprites.Get(spriteDef.Texture, spriteDef.Sprite, spriteDef.Color));
                        var spriteSize = sprite.GetLocalBounds().Size();
                        sprite.Position = (spriteDef.Offset + worldPos - viewPos) * ViewTileSize.V + Position.V;
                        sprite.Scale = ViewTileSize.V / spriteSize * spriteDef.Scale;
                        sprite.Color = Resources.Colors.Get(spriteDef.Color);
                        sprite.Origin = new Vec(0.5f, 0.5f) * spriteSize;
                        target.Draw(sprite, states);
                        anim.Enqueue(pair);
                    }
                }
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
                        if (asActor != null && drawable != Following.V)
                        {
                            layers[RenderLayerName.BackgroundEffects] += tex =>
                            {
                                var color = FactionSystem.GetRelations(Following.V, asActor).Left switch
                                {
                                    var x when x.IsHostile() => ColorName.Red,
                                    var x when x.IsFriendly() => ColorName.Green,
                                    _ => ColorName.Yellow
                                };
                                if (!Resources.Sprites.TryGet(TextureName.Icons, "AllegianceCircle", color, out var circleDef, rngSeed))
                                    return;
                                using var sprite = new Sprite(circleDef);
                                var spriteSize = sprite.GetLocalBounds().Size();
                                sprite.Origin = new Vec(0.5f, 0.5f) * spriteSize;
                                sprite.Scale = ViewTileSize.V / spriteSize;
                                sprite.Position = screenPos + spriteSize * new Vec(0f, 0.25f) * sprite.Scale.ToVec();
                                tex.Draw(sprite, states);
                            };
                        }
                        // Draw sprite
                        layers[drawable.Render.Layer] += tex =>
                        {
                            if (Resources.Sprites.TryGet(drawable.Render.Texture, drawable.Render.Sprite, drawable.Render.Color, out var spriteDef, rngSeed))
                            {
                                using var sprite = new Sprite(spriteDef);
                                var spriteSize = sprite.GetLocalBounds().Size();
                                sprite.Position = screenPos;
                                sprite.Origin = new Vec(0.5f, 0.5f) * spriteSize;
                                sprite.Scale = ViewTileSize.V / spriteSize;
                                if (!seen)
                                {
                                    sprite.Color = sprite.Color.AddRgb(-64, -64, -64);
                                }
                                tex.Draw(sprite, states);
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

        public void Animate(bool blocking, Coord worldPos, params Animation[] animations)
        {
            if (blocking)
            {
                Impl();
            }
            else
            {
                Task.Run(Impl);
            }
            void Impl()
            {
                var time = TimeSpan.Zero;
                var increment = TimeSpan.FromMilliseconds(4);
                var timeline = animations.SelectMany(Timeline)
                    .OrderBy(x => x.Time)
                    .ToList();
                var viewPos = ViewArea.V.Position();
                var myVfx = new ConcurrentQueue<OrderedPair<Coord, SpriteDef>>();
                var k = Vfx.Keys.LastOrDefault() + 1;
                Vfx[k] = myVfx;
                while (timeline.Count > 0)
                {
                    for (int i = timeline.Count - 1; i >= 0; i--)
                    {
                        var t = timeline[i];
                        if (time <= t.Time + t.Frame.Duration && time > t.Time)
                        {
                            foreach (var spriteDef in t.Frame.Sprites)
                            {
                                myVfx.Enqueue(new(worldPos, spriteDef));
                            }
                            t.Anim.OnFramePlaying(t.Index);
                        }
                        else if (time > t.Time + t.Frame.Duration)
                        {
                            timeline.RemoveAt(i);
                        }
                    }
                    if (blocking)
                    {
                        Loop.WaitAndDraw(increment);
                    }
                    else
                    {
                        new GameLoop().Run(increment);
                    }
                    time += increment;
                    myVfx.Clear();
                }
                Vfx.Remove(k, out _);
            }

            IEnumerable<(int Index, Animation Anim, TimeSpan Time, AnimationFrame Frame)> Timeline(Animation anim)
            {
                var time = TimeSpan.Zero;
                for (int i = 0; i < anim.Frames.Length; ++i)
                {
                    yield return (i, anim, time, anim.Frames[i]);
                    time += anim.Frames[i].Duration;
                }
            }
        }
    }
}
