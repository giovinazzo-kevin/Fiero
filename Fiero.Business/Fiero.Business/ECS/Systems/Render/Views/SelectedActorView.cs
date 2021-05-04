using Fiero.Core;
using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Business
{

    public class SelectedActorView : View
    {
        public Actor Following { get; set; }
        protected Layout Layout { get; private set; }
        protected Label ActorName { get; private set; }
        protected Label ActorHealth { get; private set; }
        protected Paragraph Logs { get; private set; }


        internal SelectedActorView(LayoutBuilder<FontName, TextureName, SoundName> layoutBuilder)
        {
            layoutBuilder.Label(new(1, 1), 10, "<player>", initialize: x => ActorName = x);
            layoutBuilder.Label(new(1, 2), 10, "<hp>", initialize: x => ActorHealth = x);
            layoutBuilder.Paragraph(new(1, 84), 98, 10, "<logs>", initialize: x => Logs = x);
            Layout = layoutBuilder.Build();

            Logs.Scale = new(1, 1);
        }

        public override void Update(RenderWindow win, float t, float dt)
        {
            Layout.Update(t, dt);
            if (Following != null && Following.Id != 0) {
                ActorName.Text = Following.Info.Name;
                ActorHealth.Text = $"HP: {Following.Properties.Health}/{Following.Properties.MaximumHealth}";
                ActorHealth.ActiveColor = Color.White;
                if(Following.Log != null) {
                    Logs.Text = String.Join('\n', Following.Log.GetMessages().TakeLast(Logs.MaxLines));
                }
            }
            else {
                ActorHealth.Text = $"(DEAD)";
                ActorHealth.ActiveColor = Color.Red;
                Logs.Text = String.Empty;
            }
        }

        public override void Draw(RenderWindow win, float t, float dt)
        {
            win.Draw(Layout);
        }

        public override void OnWindowResized(Coord newSize) => throw new NotImplementedException();
    }
}
