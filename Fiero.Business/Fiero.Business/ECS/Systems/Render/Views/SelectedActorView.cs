using Fiero.Core;
using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Business
{

    public class SelectedActorView : View
    {
        public UIControlProperty<Actor> Following { get; private set; } = new(nameof(Following), null);
        protected Layout TopRow { get; private set; }
        protected Layout BottomRow { get; private set; }
        protected Label ActorName { get; private set; }
        protected Label ActorHealth { get; private set; }
        protected Paragraph Logs { get; private set; }

        internal SelectedActorView(LayoutBuilder layoutBuilder)
        {
            TopRow = layoutBuilder
                .Build(new(), grid => ApplyStyles(grid)
                    .Row()
                        .Col()
                            .Cell<Label>(x => ActorName = x)
                        .End()
                        .Col()
                            .Cell<Label>(x => ActorHealth = x)
                        .End()
                    .End()
                );
            BottomRow = layoutBuilder
                .Build(new(), grid => ApplyStyles(grid)
                    .Row()
                        .Cell<Paragraph>(x => Logs = x)
                    .End()
                );
            Data.UI.WindowSize.ValueChanged += e => {
                TopRow.Size.V = new Coord(e.NewValue.X, 32);
                BottomRow.Size.V = (e.NewValue * new Vec(1, 0.15f)).ToCoord();
                BottomRow.Position.V = new Coord(0, e.NewValue.Y - BottomRow.Size.V.Y);
            };

            Following.ValueChanged += (_, __) => {
                if (Following.V != null && Following.V.Id != 0) {
                    ActorName.Text.V = Following.V.Info.Name;
                    ActorHealth.Foreground.V = Color.White;
                }
                else {
                    ActorHealth.Text.V = $"(DEAD)";
                    ActorHealth.Foreground.V = Color.Red;
                    Logs.Text.V = String.Empty;
                }
            };

            LayoutGrid ApplyStyles(LayoutGrid grid)
            {
                return grid
                    .Style<UIControl>(s => s.Apply(l => {
                        l.Background.V = Color.Transparent;
                    }))
                    .Style<Label>(s => s.Apply(l => {
                        l.Background.V = Color.Black;
                        l.FontSize.V = 24;
                        l.CenterContentH.V = false;
                    }))
                    .Style<Paragraph>(s => s.Apply(l => {
                        l.Background.V = Color.Black;
                        l.FontSize.V = 12;
                        l.MaxLines.V = 10;
                    }));
            }
        }

        public override void Update(RenderWindow win, float t, float dt)
        {
            TopRow.Update(t, dt);
            BottomRow.Update(t, dt);
            if (Following.V != null && Following.V.Id != 0) {
                ActorHealth.Text.V = $"HP: {Following.V.ActorProperties.Health}/{Following.V.ActorProperties.MaximumHealth}";
                if(Following.V.Log != null) {
                    Logs.Text.V = String.Join('\n', Following.V.Log.GetMessages().TakeLast(Logs.MaxLines));
                }
            }
        }

        public override void Draw(RenderWindow win, float t, float dt)
        {
            win.Draw(TopRow);
            win.Draw(BottomRow);
        }

        public override void OnWindowResized(Coord newSize) => throw new NotImplementedException();
    }
}
