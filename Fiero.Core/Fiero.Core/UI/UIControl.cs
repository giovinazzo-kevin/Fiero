using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Core
{
    public abstract class UIControl : Drawable
    {
        protected readonly GameInput Input;
        public readonly List<UIControl> Children;

        public event Action<UIControl, Coord> PositionChanged;
        protected virtual void OnPositionChanged(Coord oldPosition) { }
        public event Action<UIControl, bool> ActiveChanged;
        protected virtual void OnActiveChanged(bool oldActive) { }
        public event Action<UIControl, bool> HiddenChanged;
        protected virtual void OnHiddenChanged(bool oldHidden) { }
        public event Action<UIControl, Coord> Clicked;
        protected virtual void OnClicked(Coord mousePos) { }

        public bool Clickable { get; protected set; }

        private Coord _position;
        public Coord Position {
            get => _position;
            set {
                var delta = new Coord(_position.X - value.X, _position.Y - value.Y);
                var oldValue = _position;
                _position = value;
                foreach (var child in Children) {
                    child.Position = new(child.Position.X - delta.X, child.Position.Y - delta.Y);
                }
                PositionChanged?.Invoke(this, oldValue);
                OnPositionChanged(oldValue);
            }
        }

        public Coord Size { get; set; }

        private Coord _scale;
        public Coord Scale {
            get => _scale;
            set {
                _scale = value;
                foreach (var child in Children) {
                    child.Scale = value;
                }
            }
        }
        public Color ActiveColor { get; set; }
        public Color InactiveColor { get; set; }

        private volatile bool _isActive;
        public bool IsActive {
            get => !Clickable || _isActive;
            internal set {
                var oldValue = _isActive;
                _isActive = value;
                foreach (var child in Children) {
                    child.IsActive = value;
                }
                ActiveChanged?.Invoke(this, oldValue);
                OnActiveChanged(oldValue);
            }
        }
        private volatile bool _isHidden;
        public bool IsHidden {
            get => _isHidden;
            internal set {
                var oldValue = _isHidden;
                _isHidden = value;
                foreach (var child in Children) {
                    child.IsHidden = value;
                }
                HiddenChanged?.Invoke(this, oldValue);
                OnHiddenChanged(oldValue);
            }
        }

        public virtual bool Contains(Coord point, out UIControl owner)
        {
            owner = default;
            foreach (var child in Children.Where(c => c.Clickable && !c.IsHidden)) {
                if(child.Contains(point, out owner)) {
                    return true;
                }
            }
            if (point.X >= Position.X * Scale.X && point.X <= Position.X * Scale.X + Size.X * Scale.X
            && point.Y >= Position.Y * Scale.Y && point.Y <= Position.Y * Scale.Y + Size.Y * Scale.Y) {
                owner = this;
                return true;
            }
            return false;
        }

        public void Click(Coord mousePos)
        {
            if(Clickable) {
                Clicked?.Invoke(this, mousePos);
                OnClicked(mousePos);
            }
        }

        public UIControl(GameInput input)
        {
            Input = input;
            Children = new List<UIControl>();
            Scale = new Coord(1, 1);
        }

        public virtual void Update(float t, float dt)
        {
            foreach (var child in Children) {
                child.Update(t, dt);
            }
        }

        public virtual void Draw(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            foreach (var child in Children) {
                child.Draw(target, states);
            }
        }
    }
}
