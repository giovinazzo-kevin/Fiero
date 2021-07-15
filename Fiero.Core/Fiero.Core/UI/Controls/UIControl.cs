using Microsoft.VisualBasic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fiero.Core
{
    public abstract partial class UIControl : Drawable
    {
        protected readonly GameInput Input;
        public readonly List<UIControl> Children;
        public readonly IReadOnlyList<IUIControlProperty> Properties;

        public readonly UIControlProperty<bool> IsInteractive = new(nameof(IsInteractive), false);
        public readonly UIControlProperty<Coord> Snap = new(nameof(Snap), new(1, 1));
        public readonly UIControlProperty<Coord> Margin = new(nameof(Margin), new());
        public readonly UIControlProperty<Coord> Padding = new(nameof(Padding), new());
        public readonly UIControlProperty<Coord> Position = new(nameof(Position), new());
        public readonly UIControlProperty<Coord> Size = new(nameof(Size), new());
        public readonly UIControlProperty<Vec> Scale = new(nameof(Scale), new(1, 1));
        public readonly UIControlProperty<Color> Foreground = new(nameof(Foreground), new(255, 255, 255));
        public readonly UIControlProperty<Color> Background = new(nameof(Background), new(0, 0, 0, 0));
        public readonly UIControlProperty<Color> Accent = new(nameof(Accent), new(255, 0, 0));
        public readonly UIControlProperty<bool> IsHidden = new(nameof(IsHidden), false) { Propagate = true };
        public readonly UIControlProperty<bool> IsActive = new(nameof(IsActive), false) { Propagate = true };
        public readonly UIControlProperty<bool> IsMouseOver = new(nameof(IsMouseOver), false);
        public readonly UIControlProperty<int> ZOrder = new(nameof(ZOrder), 0);

        public Coord BorderRenderPos => (Position.V + Margin.V).Align(Snap);
        public Coord ContentRenderPos => (Position.V + Margin.V + Padding.V).Align(Snap);
        public Coord BorderRenderSize => (Size.V - Margin.V * 2).Align(Snap);
        public Coord ContentRenderSize => (Size.V - Margin.V * 2 - Padding.V * 2).Align(Snap);

        public void CopyProperties(UIControl from)
        {
            foreach (var fromProp in from.Properties) {
                if(Properties.SingleOrDefault(x => x.Name.Equals(fromProp.Name)) is { } myProp) {
                    myProp.Value = fromProp.Value;
                }
            }
        }


        public UIControl(GameInput input)
        {
            Input = input;
            Children = new List<UIControl>();

            Properties = GetType()
                .GetRuntimeFields()
                .Where(p => p.FieldType.GetInterface(nameof(IUIControlProperty)) != null)
                .Select(p => (IUIControlProperty)p.GetValue(this))
                .ToList();

            foreach (var prop in Properties) {
                prop.SetOwner(this);
            }
        }

        public virtual bool Contains(Coord point, out UIControl owner)
        {
            owner = default;
            if (point.X >= BorderRenderPos.X * Scale.V.X
                && point.X <= BorderRenderPos.X * Scale.V.X + BorderRenderSize.X * Scale.V.X
                && point.Y >= BorderRenderPos.Y * Scale.V.Y 
                && point.Y <= BorderRenderPos.Y * Scale.V.Y + BorderRenderSize.Y * Scale.V.Y) {
                foreach (var child in Children.Where(c => c.IsInteractive && !c.IsHidden)) {
                    if (child.Contains(point, out owner)) {
                        return true;
                    }
                }
                owner = this;
                return true;
            }
            return false;
        }

        public bool Click(Coord mousePos, Mouse.Button button)
        {
            var preventDefault = Clicked?.Invoke(this, mousePos, button) ?? false;
            return preventDefault || OnClicked(mousePos, button);
        }

        private Coord _trackedMousePosition;
        protected bool TrackMouse(Coord mousePos, out UIControl clickedControl, out Mouse.Button clickedButton)
        {
            clickedButton = default;
            clickedControl = default;
            var wasInside = Contains(_trackedMousePosition, out _);
            var isInside = Contains(mousePos, out _);
            var leftClick = Input.IsButtonPressed(Mouse.Button.Left);
            var rightClick = Input.IsButtonPressed(Mouse.Button.Right);
            var click = leftClick || rightClick;
            var preventDefault = false;
            if (!wasInside && isInside) {
                MouseEntered?.Invoke(this, mousePos);
                OnMouseEntered(mousePos);
                if (!click) {
                    foreach (var child in Children) {
                        child.TrackMouse(mousePos, out _, out _);
                    }
                }
            }
            else if (wasInside && !isInside) {
                MouseLeft?.Invoke(this, mousePos);
                OnMouseLeft(mousePos);
                if (!click) {
                    foreach (var child in Children) {
                        child.TrackMouse(mousePos, out _, out _);
                    }
                }
            }
            else if (wasInside && isInside) {
                MouseMoved?.Invoke(this, mousePos);
                OnMouseMoved(mousePos);
                if (!click) {
                    foreach (var child in Children) {
                        child.TrackMouse(mousePos, out _, out _);
                    }
                }
            }
            if (isInside && click) {
                foreach (var child in Children) {
                    if (child.Contains(mousePos, out clickedControl)) {
                        break;
                    }
                }
                clickedControl ??= this;
                clickedButton = leftClick ? Mouse.Button.Left : Mouse.Button.Right;
                preventDefault = clickedControl.Click(mousePos, clickedButton);
            }
            _trackedMousePosition = mousePos;
            return preventDefault;
        }

        public virtual void Update(float t, float dt)
        {
            foreach (var child in Children) {
                child.Update(t, dt);
            }
        }

        protected virtual void DrawBackground(RenderTarget target, RenderStates states)
        {
            var rect = new RectangleShape(BorderRenderSize.ToVector2f()) {
                Position = BorderRenderPos.ToVector2f(),
                FillColor = Background,
                OutlineThickness = 1f,
                OutlineColor = Accent
            };
            target.Draw(rect, states);
        }

        public virtual void Draw(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            DrawBackground(target, states);
            foreach (var child in Children.OrderByDescending(x => x.ZOrder.V).ThenByDescending(x => x.IsActive.V ? 0 : 1)) {
                child.Draw(target, states);
            }
        }
    }
}
