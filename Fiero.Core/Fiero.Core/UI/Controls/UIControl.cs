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

        public readonly UIControlProperty<bool> Clickable = new(nameof(Clickable), false);
        public readonly UIControlProperty<Coord> Snap = new(nameof(Snap), new(1, 1));
        public readonly UIControlProperty<Coord> Position = new(nameof(Position), new());
        public UIControlProperty<Coord> Size = new(nameof(Size), new());
        public UIControlProperty<Vec> Scale = new(nameof(Scale), new(1, 1));
        public readonly UIControlProperty<Color> Foreground = new(nameof(Foreground), new(255, 255, 255));
        public readonly UIControlProperty<Color> Background = new(nameof(Background), new(0, 0, 0));
        public readonly UIControlProperty<Color> Accent = new(nameof(Accent), new(255, 0, 0));
        public readonly UIControlProperty<bool> IsHidden = new(nameof(IsHidden), false) { Propagate = true };
        public readonly UIControlProperty<bool> IsActive = new(nameof(IsActive), false) { Propagate = true };
        public readonly UIControlProperty<bool> IsMouseOver = new(nameof(IsMouseOver), false);

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
            foreach (var child in Children.Where(c => c.Clickable && !c.IsHidden)) {
                if (child.Contains(point, out owner)) {
                    return true;
                }
            }
            if (point.X >= Position.V.X * Scale.V.X
                && point.X <= Position.V.X * Scale.V.X + Size.V.X * Scale.V.X
                && point.Y >= Position.V.Y * Scale.V.Y 
                && point.Y <= Position.V.Y * Scale.V.Y + Size.V.Y * Scale.V.Y) {
                owner = this;
                return true;
            }
            return false;
        }

        public void Click(Coord mousePos)
        {
            if (Clickable) {
                Clicked?.Invoke(this, mousePos);
                OnClicked(mousePos);
            }
        }

        private Coord _trackedMousePosition;
        protected void TrackMouse(Coord mousePos)
        {
            var wasInside = Contains(_trackedMousePosition, out _);
            var isInside = Contains(mousePos, out _);
            if (!wasInside && isInside) {
                MouseEntered?.Invoke(this, mousePos);
                OnMouseEntered(mousePos);
            }
            else if (wasInside && !isInside) {
                MouseLeft?.Invoke(this, mousePos);
                OnMouseLeft(mousePos);
            }
            else if (wasInside && isInside) {
                MouseMoved?.Invoke(this, mousePos);
                OnMouseMoved(mousePos);
            }
            if (isInside && Input.IsButtonPressed(Mouse.Button.Left)) {
                var clickedControl = default(UIControl);
                foreach (var child in Children) {
                    if (child.Contains(mousePos, out clickedControl)) {
                        break;
                    }
                }
                clickedControl ??= this;
                clickedControl.Click(mousePos);
            }
            _trackedMousePosition = mousePos;
        }

        public virtual void Update(float t, float dt)
        {
            TrackMouse(Input.GetMousePosition().ToCoord());
            foreach (var child in Children) {
                child.Update(t, dt);
            }
        }

        protected virtual void DrawBackground(RenderTarget target, RenderStates states)
        {
            var rect = new RectangleShape(Size.V.ToVector2f()) {
                Position = Position.V.ToVector2f(),
                FillColor = IsMouseOver ? Accent : Background
            };
            target.Draw(rect, states);
        }

        public virtual void Draw(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            DrawBackground(target, states);
            foreach (var child in Children) {
                child.Draw(target, states);
            }
        }
    }
}
