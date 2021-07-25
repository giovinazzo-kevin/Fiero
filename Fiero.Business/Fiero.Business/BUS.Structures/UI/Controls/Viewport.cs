﻿using Fiero.Core;
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

        public readonly UIControlProperty<FloorId> ViewFloor = new(nameof(ViewFloor), default);
        public readonly UIControlProperty<IntRect> ViewArea = new(nameof(ViewArea), new(0, 0, 40, 40));
        public readonly UIControlProperty<Coord> ViewTileSize = new(nameof(ViewTileSize), new(16, 16));
        public readonly UIControlProperty<Coord?> CursorPosition = new(nameof(CursorPosition), default);
        public readonly UIControlProperty<Color> CursorColor = new(nameof(CursorColor), Color.White);
        public readonly UIControlProperty<IEnumerable<Coord>> KnownTiles = new(nameof(KnownTiles), Enumerable.Empty<Coord>());
        public readonly UIControlProperty<IEnumerable<Coord>> VisibleTiles = new(nameof(VisibleTiles), Enumerable.Empty<Coord>());

        private RenderTexture _renderTexture;
        private Sprite _renderSprite;
        private Sprite _cursorSprite;
        private bool _dirty = true;

        public Viewport(
            GameInput input, 
            FloorSystem floor,
            GameSprites<TextureName> sprites,
            GameColors<ColorName> colors,
            Sprite cursorSprite
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
            ViewFloor.ValueChanged += (_, __) => SetDirty();
            ViewTileSize.ValueChanged += (_, __) => SetDirty();

            _cursorSprite = cursorSprite;
        }

        public void SetDirty() => _dirty = true;

        public override void Draw(RenderTarget target, RenderStates states)
        {
            base.Draw(target, states);
            if(_dirty) {
                if (!Bake())
                    return;
            }
            target.Draw(_renderSprite);
            if(CursorPosition.V is { } cur) {
                _cursorSprite.Position = (cur - new Coord(ViewArea.V.Left, ViewArea.V.Top)) * ViewTileSize.V + Position.V;
                _cursorSprite.Scale = ViewTileSize.V / _cursorSprite.GetLocalBounds().Size();
                _cursorSprite.Color = CursorColor.V;
                target.Draw(_cursorSprite);
            }
            bool Bake()
            {
                if (!FloorSystem.TryGetFloor(ViewFloor.V, out var floor))
                    return false;
                _renderTexture.Clear();
                var screenBounds = Position.V + Size.V;
                foreach (var coord in ViewArea.V.Enumerate()) {
                    if (!floor.Cells.TryGetValue(coord, out var cell))
                        continue;

                    var known = KnownTiles.V.Contains(coord);
                    var seen = VisibleTiles.V.Contains(coord);

                    if (!known)
                        continue;

                    var relativePos = coord - new Coord(ViewArea.V.Left, ViewArea.V.Top);
                    var screenPos = relativePos * ViewTileSize.V + Position.V;
                    if (
                           screenPos.X < -ViewTileSize.V.X || screenPos.X >= screenBounds.X
                        || screenPos.Y < -ViewTileSize.V.Y || screenPos.Y >= screenBounds.Y) {
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
                        if (drawable is Actor actor && (actor.Npc?.IsBoss ?? false)) {
                            sprite.Scale = ViewTileSize.V / spriteSize * 2;
                            sprite.Position -= ViewTileSize.V * new Vec(0.5f, 1f);
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