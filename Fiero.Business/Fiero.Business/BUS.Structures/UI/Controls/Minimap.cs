using Fiero.Core;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class Minimap : UIControl
    {
        protected readonly FloorSystem FloorSystem;
        protected readonly FactionSystem FactionSystem;
        protected readonly GameColors<ColorName> Colors;

        public readonly UIControlProperty<Actor> Following = new(nameof(Following), null);

        private RenderTexture _renderTexture;
        private Sprite _renderSprite;
        private bool _dirty = true;

        public Minimap(
            GameInput input,
            FloorSystem floor,
            FactionSystem faction,
            GameColors<ColorName> colors
        ) : base(input)
        {
            FloorSystem = floor;
            FactionSystem = faction;
            Colors = colors;
            Size.ValueChanged += (_, __) => {
                _renderTexture?.Dispose();
                _renderSprite?.Dispose();
                _renderTexture = new((uint)Size.V.X, (uint)Size.V.Y) { Smooth = false };
                _renderSprite = new(_renderTexture.Texture);
                SetDirty();
            };
            Following.ValueChanged += (_, __) => SetDirty();
        }

        public void SetDirty() => _dirty = true;

        public override void Draw(RenderTarget target, RenderStates states)
        {
            base.Draw(target, states);
            if (_dirty && Following.V != null) {
                if (!Bake())
                    return;
            }
            target.Draw(_renderSprite);
            bool Bake()
            {
                var floorId = Following.V.FloorId();
                if (!FloorSystem.TryGetFloor(floorId, out var floor))
                    return false;
                _renderTexture.Clear(Background.V);
                using var whitePixel = new RenderTexture(1, 1);
                whitePixel.Clear(Color.White);
                whitePixel.Display();
                foreach (var coord in floor.Size.ToRect().Enumerate()) {
                    if (!floor.Cells.TryGetValue(coord, out var cell))
                        continue;

                    var known = Following.V.Fov.KnownTiles[floorId].Contains(coord);
                    var seen = true || Following.V.Fov.VisibleTiles[floorId].Contains(coord);
                    if (false && !known)
                        continue;
                    if (
                           coord.X < 0 || coord.X >= Size.V.X
                        || coord.Y < 0 || coord.Y >= Size.V.Y) {
                        continue;
                    }

                    foreach (var drawable in cell.GetDrawables(seen)) {
                        if (drawable.Render.Hidden)
                            continue;
                        using var sprite = new Sprite(whitePixel.Texture);
                        sprite.Color = Colors.Get(drawable switch {
                            Tile x when x.TileProperties.Name == TileName.Corridor => ColorName.Magenta,
                            Tile x when x.Physics.BlocksMovement => ColorName.White,
                            Tile x when !x.Physics.BlocksMovement => ColorName.Blue,
                            Item x => ColorName.LightCyan,
                            Feature x when x.FeatureProperties.Name == FeatureName.Trap => ColorName.LightGreen,
                            Feature x when x.FeatureProperties.Name == FeatureName.Downstairs => ColorName.LightMagenta,
                            Feature x when x.FeatureProperties.Name == FeatureName.Upstairs => ColorName.Magenta,
                            Actor x when x == Following.V => ColorName.White,
                            Actor x when FactionSystem.GetRelationships(x, Following).Left.IsFriendly() => ColorName.LightYellow,
                            Actor x when FactionSystem.GetRelationships(x, Following).Left.IsHostile() => ColorName.LightRed,
                            Actor x => ColorName.LightGray,
                            PhysicalEntity x when x.Physics.BlocksMovement => ColorName.Gray,
                            _ => ColorName.Black
                        });
                        sprite.Position = coord + Coord.PositiveOne;
                        var spriteSize = sprite.GetLocalBounds().Size();
                        sprite.Origin = new Vec(0.5f, 0.5f) * spriteSize;
                        sprite.Scale = Coord.PositiveOne / spriteSize;
                        _renderTexture.Draw(sprite, states);
                    }
                }
                _renderTexture.Display();
                _renderSprite.Position = Position.V;
                _renderSprite.Scale = Size.V / floor.Size;
                _dirty = false;
                return true;
            }
        }
    }
}
