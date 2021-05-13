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

        public HealthbarDisplayView(LayoutBuilder layoutBuilder)
        {
            //layoutBuilder.ProgressBar(new(), 3, 0, initialize: x => EnemyBar = x);
            //layoutBuilder.ProgressBar(new(), 6, 0, initialize: x => BossBar = x);
            //layoutBuilder.Build();
            //EnemyBar.Scale = new(1, 1);
            //EnemyBar.Center = true;
            //BossBar.Scale = new(1, 1);
            //BossBar.Center = true;

            layoutBuilder.Build(new(), grid => grid
                .Row()
                    .Cell<ProgressBar>(x => {
                        EnemyBar = x;
                        x.Center.V = true;
                        x.Length = 3;
                    })
                .End()
                .Row()
                    .Cell<ProgressBar>(x => {
                        BossBar = x;
                        x.Center.V = true;
                        x.Length = 6;
                    })
                .End()
            );
        }

        public override void Draw(RenderWindow win, float t, float dt)
        {
            if (Following is null || Following.Id == 0)
                return;
            if (Following.Properties.Health == Following.Properties.MaximumHealth)
                return;
            var bar = Following.Properties.IsBoss
                ? BossBar : EnemyBar;
            bar.Position.V = Position;
            bar.Progress.V = Following.Properties.MaximumHealth > 0
                ? Following.Properties.Health / (float)Following.Properties.MaximumHealth 
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
            win.Draw(bar);
        }

        public override void OnWindowResized(Coord newSize) => throw new System.NotImplementedException();
    }
}
