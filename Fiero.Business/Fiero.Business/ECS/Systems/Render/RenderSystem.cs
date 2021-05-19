using Fiero.Core;
using SFML.Graphics;
using SFML.System;
using System;
using System.Drawing;
using System.Linq;

namespace Fiero.Business
{
    public class RenderSystem
    {
        protected readonly GameUI UI;
        protected readonly GameSprites<TextureName> Sprites;
        protected readonly GameEntities Entities;
        protected readonly GameDataStore Store;
        protected readonly GameInput Input;
        protected readonly FloorSystem FloorSystem;

        public SelectedActorView SelectedActor { get; private set; }
        public HealthbarDisplayView HealthbarDisplay { get; private set; }

        public RenderSystem(
            GameUI ui,
            GameSprites<TextureName> sprites,
            GameEntities entities,
            GameDataStore store,
            FloorSystem floor,
            GameInput input
        ) {
            UI = ui;
            Sprites = sprites;
            Entities = entities;
            Store = store;
            Input = input;
            FloorSystem = floor;
        }

        public void Initialize()
        {
            var tileSize = Store.GetOrDefault(Data.UI.TileSize, 8);
            SelectedActor = new SelectedActorView(UI.CreateLayout());
            HealthbarDisplay = new HealthbarDisplayView(UI.CreateLayout());
        }

        public void Update(RenderWindow win, float t, float dt)
        {
            SelectedActor.Update(win, t, dt);
        }

        public void Draw(RenderWindow win, float t, float dt)
        {
            var tileSize = Store.Get(Data.UI.TileSize);
            var mapCenter = FloorSystem.CurrentFloor.Tiles.Keys
                .Aggregate((a, b) => (a + b) / 2);
            var winSize = win.Size.ToVec();
            var drawables = FloorSystem.CurrentFloor.GetDrawables()
                ?? Enumerable.Empty<Drawable>();
            // If the player dies focus on the killer if one is available
            if(SelectedActor.Following == null || SelectedActor.Following.Id == 0) {
                if(Store.TryGetValue(Data.Player.KilledBy, out var killer) && killer != null && killer.Id != 0) {
                    SelectedActor.Following = killer;
                }
            }
            // If no actor to follow is available focus on the geometric center of the map
            var followPos = (SelectedActor.Following?.Physics?.Position ?? mapCenter).ToVec();
            var origin = winSize / 4f + followPos * tileSize - winSize / 2f;
            foreach (var drawable in drawables) {
                var spriteSize = drawable.Render.Sprite.TextureRect.Size().ToVec();
                var spriteScale = drawable.Render.Sprite.Scale.ToVec();
                // Center the sprite (the last term centers 2x2 sprites horizontally so that they match with the tile they're on)
                drawable.Render.Sprite.Origin = origin + spriteSize * new Vec(0.5f, 1);
                drawable.Render.Sprite.Position = drawable.Physics.Position * spriteScale * tileSize;
                win.Draw(drawable.Render.Sprite);
                if(drawable is Actor actor) {
                    var spritePosition = drawable.Render.Sprite.Transform.TransformPoint(0, 0).ToCoord();
                    // Place the health bar right above this sprite
                    HealthbarDisplay.Position = spritePosition - new Coord(0, tileSize);
                    HealthbarDisplay.Following = actor;
                    HealthbarDisplay.Draw(win, t, dt);
                    if(actor.Npc != null) {
                        if(actor.Npc.IsBoss) {
                            //drawable.Render.Sprite.
                        }
                        else {

                        }
                    }
                }
            }
            SelectedActor.Draw(win, t, dt);
        }
    }
}
