using Fiero.Core;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    /// <summary>
    /// Represents a drawable view that encompasses all tiles, items, features and actors within the specified bounds.
    /// </summary>
    public class Viewport : UIControl
    {
        protected readonly FloorSystem FloorSystem;
        protected readonly GameSprites<TextureName> Sprites;
        protected readonly GameColors<ColorName> Colors;

        public readonly UIControlProperty<IntRect> ViewArea = new(nameof(ViewArea), new(0, 0, 40, 40));
        public readonly UIControlProperty<Coord> ViewTileSize = new(nameof(ViewTileSize), new(16, 16));
        public readonly UIControlProperty<TargetingShape?> TargetingShape = new(nameof(TargetingShape), default);
        public readonly UIControlProperty<Actor> Following = new(nameof(Following), null);

        private RenderTexture _renderTexture;
        private Sprite _renderSprite;
        private bool _dirty = true;

        public Viewport(
            GameInput input, 
            FloorSystem floor,
            GameSprites<TextureName> sprites,
            GameColors<ColorName> colors
        ) : base(input)
        {
            FloorSystem = floor;
            Sprites = sprites;
            Colors = colors;
            Size.ValueChanged += (_, __) => {
                _renderTexture?.Dispose();
                _renderSprite?.Dispose();
                _renderTexture = new((uint)Size.V.X, (uint)Size.V.Y) { Smooth = false };
                _renderSprite = new(_renderTexture.Texture);
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
            if(_dirty) {
                if (!Bake())
                    return;
            }
            if (TargetingShape.V is { } shape) {
                using var darkerSprite = new Sprite(_renderSprite);
                darkerSprite.Color = new(128, 128, 128);
                target.Draw(darkerSprite);

                foreach (var point in shape.Points) {
                    var pos = (point - new Coord(ViewArea.V.Left, ViewArea.V.Top)) * ViewTileSize.V + Position.V;
                    var origin = new Vec(0.5f, 0.5f) * ViewTileSize.V;
                    var spriteRect = new IntRect(pos.X - (int)origin.X, pos.Y - (int)origin.Y, ViewTileSize.V.X, ViewTileSize.V.Y);
                    using var sprite = new Sprite(_renderSprite.Texture, spriteRect) {
                        Position = pos,
                        Origin = origin
                    };
                    using var highlight = new RectangleShape(ViewTileSize.V) {
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
            else {
                target.Draw(_renderSprite);
            }
            bool Bake()
            {
                var floorId = Following.V.FloorId();
                if (!FloorSystem.TryGetFloor(floorId, out var floor))
                    return false;
                _renderTexture.Clear(Background.V);
                var screenBounds = Position.V + Size.V;
                var area = new IntRect(ViewArea.V.Left, ViewArea.V.Top, ViewArea.V.Width + 1, ViewArea.V.Height + 1);
                foreach (var coord in area.Enumerate()) {
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
                        || screenPos.Y < -ViewTileSize.V.Y || screenPos.Y >= screenBounds.Y + ViewTileSize.V.Y) {
                        continue;
                    }

                    foreach (var drawable in cell.GetDrawables(seen)) {
                        if (drawable.Render.Hidden)
                            continue;
                        var rngSeed = drawable.GetHashCode(); // Makes sure that randomized sprites stay consistent
                        if(!Sprites.TryGet(drawable.Render.TextureName, drawable.Render.SpriteName, out var spriteDef, rngSeed)) {
                            continue;
                        }
                        using var sprite = new Sprite(spriteDef);
                        sprite.Color = Colors.Get(drawable.Render.Color);
                        sprite.Position = screenPos;
                        var spriteSize = sprite.GetLocalBounds().Size();
                        sprite.Origin = new Vec(0.5f, 0.5f) * spriteSize;
                        if (drawable is Actor actor && actor.Npc != null) {
                            sprite.Scale = ViewTileSize.V / spriteSize;
                            //sprite.Position -= ViewTileSize.V * new Vec(0.5f, 0.5f);
                        }
                        else {
                            sprite.Scale = ViewTileSize.V / spriteSize;
                        }
                        if (!seen) {
                            sprite.Color = sprite.Color.AddRgb(-64, -64, -64);
                        }
                        _renderTexture.Draw(sprite, states);
                    }
                }
                _renderTexture.Display();
                _dirty = false;
                return true;
            }
        }
    }
}
