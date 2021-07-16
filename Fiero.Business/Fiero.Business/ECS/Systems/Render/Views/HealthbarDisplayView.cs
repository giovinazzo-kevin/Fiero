using Fiero.Core;
using LightInject;
using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Fiero.Business
{
    public class HealthbarDisplayView : View
    {
        public Coord Position { get; set; }
        public Actor Following { get; set; }

        protected ProgressBar EnemyBar { get; private set; }
        protected ProgressBar BossBar { get; private set; }

        public HealthbarDisplayView(GameWindow win, LayoutBuilder layoutBuilder)
            : base(win)
        {
            layoutBuilder.Build(new(), grid => grid
                .Row()
                    .Cell<ProgressBar>(x => {
                        EnemyBar = x;
                        x.Center.V = true;
                        x.Length.V = 3;
                    })
                .End()
            );
            layoutBuilder.Build(new(), grid => grid
                .Row()
                    .Cell<ProgressBar>(x => {
                        BossBar = x;
                        x.Center.V = true;
                        x.Length.V = 5;
                        x.Origin.V = new Vec(1f, 0);
                    })
                .End()
            );
        }

        public override void Draw()
        {
            if (Following is null || Following.Id == 0)
                return;
            if (Following.ActorProperties.Health == Following.ActorProperties.MaximumHealth)
                return;
            var bar = (Following.Npc?.IsBoss ?? false)
                ? BossBar : EnemyBar;
            bar.Position.V = Position;
            bar.Progress.V = Following.ActorProperties.MaximumHealth > 0
                ? Following.ActorProperties.Health / (float)Following.ActorProperties.MaximumHealth 
                : 0;
            if (bar.Progress > 1) {
                bar.Foreground.V = new(255, 0, 255);
            }
            else if (bar.Progress >= 0.75) {
                bar.Foreground.V = new(0, 255, 0);
            }
            else if (bar.Progress >= 0.66) {
                bar.Foreground.V = new(200, 255, 0);
            }
            else if (bar.Progress >= 0.50) {
                bar.Foreground.V = new(200, 200, 0);
            }
            else if (bar.Progress >= 0.33) {
                bar.Foreground.V = new(255, 0, 0);
            }
            else if(bar.Progress >= 0.0) {
                bar.Foreground.V = new(128, 0, 0);
            }
            Window.Draw(bar);
        }

        public override void OnWindowResized(Coord newSize) => throw new System.NotImplementedException();
    }
}
