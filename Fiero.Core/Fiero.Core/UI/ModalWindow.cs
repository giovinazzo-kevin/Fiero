using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Core
{
    public abstract class ModalWindow
    {
        public readonly GameUI UI;
        public Layout Layout { get; private set; }

        public UIControlProperty<string> Title { get; private set; }

        public event Action<ModalWindow, ModalWindowButtons> Closed;
        public event Action<float, float> Updated;


        public virtual void Open(string title, ModalWindowButtons buttons)
        {
            var enabledButtons = Enum.GetValues<ModalWindowButtons>()
                .Skip(1)
                .Where(v => buttons.HasFlag(v))
                .ToArray();

            Layout = UI.CreateLayout().Build(new(), grid => ApplyStyles(grid)
                .Col(@class: "modal")
                    .Row(h: 0.20f, @class: "modal-title")
                        .Cell<Label>(l => {
                            l.Text.V = title;
                            l.CenterContentH.V = true;
                            Title = l.Text;
                        })
                    .End()
                    .Row(h: 2.60f, @class: "modal-content")
                        .Repeat(1, (i, g) => RenderContent(g))
                    .End()
                    .Row(h: 0.20f, @class: "modal-controls")
                        .Repeat(enabledButtons.Length, (i, grid) => grid
                            .Col()
                                .Cell<Button>(b => {
                                    b.Text.V = enabledButtons[i].ToString();
                                    b.CenterContentH.V = true;
                                    b.Clicked += (_, __, ___) => {
                                        Close(enabledButtons[i]);
                                        return false;
                                    };
                                })
                            .End()
                        )
                    .End()
                .End()
            );
        }

        public ModalWindow(GameUI ui) 
        {
            UI = ui;
        }

        private LayoutGrid ApplyStyles(LayoutGrid grid)
        {
            var styleBuilder = DefineStyles(new LayoutStyleBuilder());
            var styles = styleBuilder.Build();
            foreach (var s in styles) {
                grid = grid.Style(s);
            }
            return grid;
        }

        protected virtual LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder)
        {
            return builder;
        }

        protected virtual LayoutGrid RenderContent(LayoutGrid layout)
        {
            return layout;
        }

        public virtual void Close(ModalWindowButtons buttonPressed)
        {
            Closed?.Invoke(this, buttonPressed);
        }

        public virtual void Update(RenderWindow win, float t, float dt)
        {
            Layout.Update(t, dt);
            Updated?.Invoke(t, dt);
        }

        public virtual void Draw(RenderWindow win, float t, float dt)
        {
            win.Draw(Layout);
        }
    }
}
