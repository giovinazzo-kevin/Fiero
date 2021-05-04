using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Core
{
    public class Combobox<TValue> : Label
    {
        protected readonly struct Option
        {
            public readonly ComboItem Control;
            public readonly string Label;
            public readonly TValue Value;

            public Option(string label, TValue value, ComboItem control)
            {
                Control = control;
                Label = label;
                Value = value;
            }
        }

        protected readonly Func<ComboItem> BuildItem;
        protected readonly HashSet<Option> Options;

        protected Option SelectedOption { get; private set; }

        private void Control_Clicked(UIControl option, Coord mousePos)
        {
            SelectedOption = Options.FirstOrDefault(o => o.Control == option);
            Text = SelectedOption.Label;
        }

        public Combobox<TValue> AddOption(string label, TValue value)
        {
            Clickable = true;
            var control = BuildItem();
            Children.Add(control);
            Options.Add(new Option(label, value, control));
            control.ActiveColor = ActiveColor;
            control.InactiveColor = InactiveColor;
            control.Text = label;
            control.IsHidden = true;
            control.Position = new(Position.X, Position.Y + Options.Count * (Size.Y + 4));
            control.Size = Size;
            control.Clicked += Control_Clicked;
            Control_Clicked(control, new());
            return this;
        }


        public Combobox<TValue> RemoveOptions(Func<TValue, bool> predicate)
        {
            var toRemove = Options.Where(o => predicate(o.Value));
            Children.RemoveAll(c => toRemove.Select(r => r.Control).Contains(c));
            Options.RemoveWhere(o => toRemove.Select(r => r.Value).Contains(o.Value));
            foreach (var remove in toRemove) {
                remove.Control.Clicked -= Control_Clicked;
            }
            // Recalculate positons
            var i = 0; foreach (var option in Options) { 
                option.Control.Position = new(Position.X, Position.Y + ++i * (Size.Y + 4));
            }
            return this;
        }

        protected override void OnActiveChanged(bool oldActive)
        {
            if(IsActive) {
                foreach (var option in Options) {
                    option.Control.IsHidden = false;
                    option.Control.IsActive = false;
                }
            }
            else {
                foreach (var option in Options) {
                    option.Control.IsHidden = true;
                }
            }
        }

        public Combobox(GameInput input, Frame frame, int maxLength, Func<string, Text> getText, Func<ComboItem> buildItem) : base(input, maxLength, getText)
        {
            BuildItem = buildItem;
            Options = new HashSet<Option>();
            Children.Add(frame);
        }
    }
}
