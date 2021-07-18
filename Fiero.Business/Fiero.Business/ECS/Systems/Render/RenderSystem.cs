using Fiero.Core;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unconcern.Common;

namespace Fiero.Business
{
    public partial class RenderSystem : EcsSystem
    {
        protected readonly GameUI UI;
        protected readonly GameSprites<TextureName> Sprites;
        protected readonly GameEntities Entities;
        protected readonly GameDataStore Store;
        protected readonly GameInput Input;
        protected readonly GameLoop Loop;
        protected readonly FloorSystem FloorSystem;

        public SelectedActorView SelectedActor { get; private set; }
        public HealthbarDisplayView HealthbarDisplay { get; private set; }
        public Coord Zoom { get; private set; } = new(1, 1);
        protected readonly List<Sprite> Vfx = new();

        public readonly SystemEvent<RenderSystem, AnimationFramePlayedEvent> AnimationFramePlayed;
        public readonly SystemEvent<RenderSystem, FrameEvent> DrawStarted;
        public readonly SystemEvent<RenderSystem, FrameEvent> DrawEnded;

        public RenderSystem(
            EventBus bus,
            GameUI ui,
            GameSprites<TextureName> sprites,
            GameEntities entities,
            GameDataStore store,
            FloorSystem floor,
            GameLoop loop,
            GameInput input
        ) : base(bus) {
            UI = ui;
            Sprites = sprites;
            Entities = entities;
            Store = store;
            Input = input;
            Loop = loop;
            FloorSystem = floor;

            AnimationFramePlayed = new(this, nameof(AnimationFramePlayed));
            DrawStarted = new(this, nameof(DrawStarted));
            DrawEnded = new(this, nameof(DrawEnded));
        }

        public void Initialize()
        {
            SelectedActor = new SelectedActorView(UI.Window, UI.CreateLayout());
            HealthbarDisplay = new HealthbarDisplayView(UI.Window, UI.CreateLayout());
        }

        public void UpdateViews()
        {
            SelectedActor.Update();
        }

        public void Play(Coord position, params Animation[] animations)
        {
            var time = TimeSpan.Zero;
            var increment = TimeSpan.FromMilliseconds(10);
            var timeline = animations.SelectMany(a => Timeline(a))
                .OrderBy(x => x.Time)
                .ToList();

            while(timeline.Count > 0) {
                for (int i = timeline.Count - 1; i >= 0; i--) {
                    var t = timeline[i];
                    if (time < t.Time + t.Frame.Duration && time >= t.Time) {
                        foreach (var spriteDef in t.Frame.Sprites) {
                            var sprite = Sprites.Get(spriteDef.Texture, spriteDef.Sprite);
                            sprite.Position = (position + spriteDef.Offset) * UI.Store.Get(Data.UI.TileSize);
                            sprite.Scale = Zoom;
                            sprite.Color = spriteDef.Tint;
                            Vfx.Add(sprite);
                        }
                        t.Anim.OnFramePlaying(t.Index);
                        AnimationFramePlayed.Raise(new(t.Anim, t.Index));
                    }
                    else if(time >= t.Time + t.Frame.Duration) {
                        timeline.RemoveAt(i);
                    }
                }
                Loop.WaitAndDraw(increment, (float)increment.TotalSeconds);
                time += increment;
                Vfx.Clear();
            }

            IEnumerable<(int Index, Animation Anim, TimeSpan Time, AnimationFrame Frame)> Timeline(Animation anim)
            {
                var time = TimeSpan.Zero;
                for(int i = 0; i < anim.Frames.Length; ++i) {
                    yield return (i, anim, time, anim.Frames[i]);
                    time += anim.Frames[i].Duration;
                }
            }
        }

        public void Draw()
        {
            DrawStarted.Raise(new());
            var tileSize = Store.Get(Data.UI.TileSize);
            var floorId = SelectedActor.Following.V?.ActorProperties.FloorId
                ?? FloorSystem.GetAllFloors().First().Id;
            var mapCenter = FloorSystem.GetFloor(floorId).Size / 2;
            var winSize = UI.Window.Size.ToVec();
            var drawables = FloorSystem.GetDrawables(floorId)
                ?? Enumerable.Empty<Drawable>();
            // If the player dies focus on the killer if one is available
            if(SelectedActor.Following == null || SelectedActor.Following.V.Id == 0) {
                if(Store.TryGetValue(Data.Player.KilledBy, out var killer) && killer != null && killer.Id != 0) {
                    SelectedActor.Following.V = killer;
                }
            }
            // If no actor to follow is available focus on the geometric center of the map
            var followPos = (SelectedActor.Following.V?.Physics?.Position ?? mapCenter).ToVec();
            var origin = followPos * tileSize - winSize / 2f;
            foreach (var drawable in drawables) {
                var spriteSize = drawable.Render.Sprite.TextureRect.Size().ToVec();
                var spriteScale = drawable.Render.Sprite.Scale.ToVec() * Zoom;
                // Center the sprite (the last term centers 2x2 sprites horizontally so that they match with the tile they're on)
                drawable.Render.Sprite.Origin = origin + spriteSize * new Vec(0.5f, 1);
                drawable.Render.Sprite.Position = drawable.Physics.Position * spriteScale * tileSize;
                UI.Window.Draw(drawable.Render.Sprite);
                if(drawable is Actor actor) {
                    var spritePosition = drawable.Render.Sprite.Transform.TransformPoint(0, 0).ToCoord();
                    // Place the health bar right above this sprite
                    HealthbarDisplay.Position = spritePosition - new Coord(0, tileSize);
                    HealthbarDisplay.Following = actor;
                    HealthbarDisplay.Draw();
                }
            }
            foreach (var vfx in Vfx) {
                vfx.Origin = origin + vfx.TextureRect.Size().ToVec() * new Vec(0.5f, 1);
                UI.Window.Draw(vfx);
            }
            SelectedActor.Draw();
            DrawEnded.Raise(new());
        }
    }
}
