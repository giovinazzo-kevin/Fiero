﻿using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Core
{
    public class Combobox : Label
    {
        public readonly struct Option
        {
            public readonly ComboItem Control;
            public readonly string Label;
            public readonly object Value;

            public Option(string label, object value, ComboItem control)
            {
                Control = control;
                Label = label;
                Value = value;
            }
        }

        protected readonly Func<ComboItem> BuildItem;
        protected readonly List<Option> Options;

        public Option SelectedOption { get; private set; }

        public event Action<UIControl, Option> ValueChanged;
        protected virtual bool OnValueChanged(Option item) { return false; }

        private bool Control_Clicked(UIControl option, Coord mousePos, Mouse.Button button)
        {
            SelectedOption = Options.FirstOrDefault(o => o.Control == option);
            Text.V = SelectedOption.Label;
            ValueChanged?.Invoke(this, SelectedOption);
            OnValueChanged(SelectedOption);
            return false;
        }

        public void SelectOption(Func<Option, bool> choose)
        {
            SelectedOption = Options.Single(choose);
            Text.V = SelectedOption.Label;
            ValueChanged?.Invoke(this, SelectedOption);
            OnValueChanged(SelectedOption);
        }

        public Combobox AddOption<T>(string label, T value)
        {
            IsInteractive.V = true;
            var control = BuildItem();
            Children.Add(control);
            Options.Add(new Option(label, value, control));
            control.InheritProperties(this);
            control.Text.V = label;
            control.IsHidden.V = true;
            control.Position.V = new(Position.V.X, Position.V.Y + Options.Count * (Size.V.Y + 4));
            control.Clicked += Control_Clicked;
            Control_Clicked(control, new(), Mouse.Button.Left);

            Position.ValueChanged += (_, __) => {
                Resize();
            };
            Size.ValueChanged += (_, __) => {
                Resize();
            };
            return this;

            void Resize()
            {
                control.Position.V = new(Position.V.X, Position.V.Y + Options.Count * (Size.V.Y + 4));
                control.Size.V = Size;
            }
        }

        public Combobox RemoveOptions(Func<object, bool> predicate)
        {
            var toRemove = Options.Where(o => predicate(o.Value));
            Children.RemoveAll(c => toRemove.Select(r => r.Control).Contains(c));
            Options.RemoveAll(o => toRemove.Select(r => r.Value).Contains(o.Value));
            foreach (var remove in toRemove) {
                remove.Control.Clicked -= Control_Clicked;
            }
            // Recalculate positons
            var i = 0; foreach (var option in Options) { 
                option.Control.Position.V = new(Position.V.X, Position.V.Y + ++i * (Size.V.Y + 4));
            }
            return this;
        }

        public override void Update()
        {
            if (IsMouseOver && Input.IsKeyPressed(Keyboard.Key.Up)) {
                var i = Options.IndexOf(SelectedOption);
                SelectOption(x => x.Control == Options[(i + 1) % Options.Count].Control);
            }
            if (IsMouseOver && Input.IsKeyPressed(Keyboard.Key.Down)) {
                var i = Options.IndexOf(SelectedOption);
                SelectOption(x => x.Control == Options[((i - 1 + Options.Count) % Options.Count) % Options.Count].Control);
            }
        }

        public Combobox(GameInput input, Func<string, BitmapText> getText, Func<ComboItem> buildItem) : base(input, getText)
        {
            BuildItem = buildItem;
            Options = new List<Option>();

            //IsActive.ValueChanged += (owner, old) => {
            //    if (IsActive.V) {
            //        foreach (var option in Options) {
            //            option.Control.IsHidden.V = false;
            //            option.Control.IsActive.V = false;
            //        }
            //    }
            //    else {
            //        foreach (var option in Options) {
            //            option.Control.IsHidden.V = true;
            //        }
            //    }
            //};
        }
    }
}
