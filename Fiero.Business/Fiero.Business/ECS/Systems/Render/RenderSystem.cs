﻿using Fiero.Core;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unconcern.Common;

namespace Fiero.Business
{
    public class RenderSystem : EcsSystem
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
        }

        public void Initialize()
        {
            SelectedActor = new SelectedActorView(UI.Window, UI.CreateLayout());
            HealthbarDisplay = new HealthbarDisplayView(UI.Window, UI.CreateLayout());
        }

        public void Update()
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
                    if (t.Time <= time) {
                        var sprite = Sprites.Get(t.Frame.Texture, t.Frame.Sprite);
                        sprite.Position = position + t.Frame.Offset;
                        UI.Window.Draw(sprite);
                        timeline.RemoveAt(i);
                    }
                }
                Loop.Wait(increment);
                time += increment;
            }


            IEnumerable<(TimeSpan Time, AnimationFrame Frame)> Timeline(Animation anim)
            {
                var time = TimeSpan.Zero;
                foreach (var frame in anim.Frames) {
                    yield return (time, frame);
                    time += frame.Duration;
                }
            }
        }

        public void Draw()
        {
            var tileSize = Store.Get(Data.UI.TileSize);
            var mapCenter = FloorSystem.CurrentFloor.Tiles.Keys
                .Aggregate((a, b) => (a + b) / 2);
            var winSize = UI.Window.Size.ToVec();
            var drawables = FloorSystem.CurrentFloor.GetDrawables()
                ?? Enumerable.Empty<Drawable>();
            // If the player dies focus on the killer if one is available
            if(SelectedActor.Following == null || SelectedActor.Following.V.Id == 0) {
                if(Store.TryGetValue(Data.Player.KilledBy, out var killer) && killer != null && killer.Id != 0) {
                    SelectedActor.Following.V = killer;
                }
            }
            // If no actor to follow is available focus on the geometric center of the map
            var followPos = (SelectedActor.Following.V?.Physics?.Position ?? mapCenter).ToVec();
            var origin = winSize / 4f + followPos * tileSize - winSize / 2f;
            foreach (var drawable in drawables) {
                var spriteSize = drawable.Render.Sprite.TextureRect.Size().ToVec();
                var spriteScale = drawable.Render.Sprite.Scale.ToVec();
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
                    if(actor.Npc != null) {
                        if(actor.Npc.IsBoss) {
                            //drawable.Render.Sprite.
                        }
                        else {

                        }
                    }
                }
            }
            SelectedActor.Draw();
        }
    }
}
