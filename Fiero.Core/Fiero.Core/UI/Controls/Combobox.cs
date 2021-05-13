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
            Text.V = SelectedOption.Label;
        }

        public Combobox<TValue> AddOption(string label, TValue value)
        {
            Clickable.V = true;
            var control = BuildItem();
            Children.Add(control);
            Options.Add(new Option(label, value, control));
            control.Foreground.V = Foreground;
            control.Text.V = label;
            control.IsHidden.V = true;
            control.Position.V = new(Position.V.X, Position.V.Y + Options.Count * (Size.V.Y + 4));
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
                option.Control.Position.V = new(Position.V.X, Position.V.Y + ++i * (Size.V.Y + 4));
            }
            return this;
        }

        public Combobox(GameInput input, Func<string, int, Text> getText, Func<ComboItem> buildItem) : base(input, getText)
        {
            BuildItem = buildItem;
            Options = new HashSet<Option>();

            IsActive.ValueChanged += (owner, old) => {
                if (IsActive.V) {
                    foreach (var option in Options) {
                        option.Control.IsHidden.V = false;
                        option.Control.IsActive.V = false;
                    }
                }
                else {
                    foreach (var option in Options) {
                        option.Control.IsHidden.V = true;
                    }
                }
            };
        }
    }
}
