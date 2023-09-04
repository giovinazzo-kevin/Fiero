using SFML.Graphics;
using SFML.Window;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Fiero.Core
{
    public abstract partial class UIControl : Drawable, IDisposable
    {
        public readonly GameInput Input;
        public readonly ObservableCollection<UIControl> Children;
        public readonly IReadOnlyList<IUIControlProperty> Properties;

        public readonly UIControlProperty<bool> IsInteractive = new(nameof(IsInteractive), false) { Inherited = false };
        public readonly UIControlProperty<Coord> Snap = new(nameof(Snap), new(1, 1));
        public readonly UIControlProperty<Coord> Margin = new(nameof(Margin), new()) { Inherited = false };
        public readonly UIControlProperty<Coord> Padding = new(nameof(Padding), new()) { Inherited = false };
        public readonly UIControlProperty<Coord> Position = new(nameof(Position), new());
        public readonly UIControlProperty<Coord> Size = new(nameof(Size), new(), invalidate: true);
        public readonly UIControlProperty<Vec> Origin = new(nameof(Origin), new());
        public readonly UIControlProperty<Vec> Scale = new(nameof(Scale), new(1, 1), invalidate: true);
        public readonly UIControlProperty<Color> Foreground = new(nameof(Foreground), new(255, 255, 255));
        public readonly UIControlProperty<Color> Background = new(nameof(Background), new(0, 0, 0, 0));
        public readonly UIControlProperty<Color> Accent = new(nameof(Accent), new(255, 0, 0));
        public readonly UIControlProperty<bool> IsHidden = new(nameof(IsHidden), false) { Propagated = true };
        public readonly UIControlProperty<bool> IsActive = new(nameof(IsActive), false) { Propagated = true };
        public readonly UIControlProperty<bool> IsMouseOver = new(nameof(IsMouseOver), false);
        public readonly UIControlProperty<int> ZOrder = new(nameof(ZOrder), 0);
        public readonly UIControlProperty<Color> OutlineColor = new(nameof(OutlineColor), new(255, 255, 255)) { Inherited = false };
        public readonly UIControlProperty<float> OutlineThickness = new(nameof(OutlineThickness), 0) { Inherited = false };

        public event Action<UIControl> Invalidated;
        private RenderTexture _target;
        private bool _isDirty = true;

        public void Invalidate()
        {
            Invalidated?.Invoke(this);
            _isDirty = true;
        }

        public Coord BorderRenderPos => ((Position.V + Margin.V).Align(Snap) + new Vec(OutlineThickness.V, OutlineThickness.V)).ToCoord();
        public Coord ContentRenderPos => ((Position.V + Margin.V + Padding.V).Align(Snap) + new Vec(OutlineThickness.V, OutlineThickness.V)).ToCoord();
        public Coord BorderRenderSize => ((Size.V - Margin.V * 2).Align(Snap));
        public Coord ContentRenderSize => ((Size.V - Margin.V * 2 - Padding.V * 2).Align(Snap) - new Vec(OutlineThickness.V, OutlineThickness.V) * 2).ToCoord();
        protected Coord TrackedMousePosition { get; private set; }

        // Copies all matching and propagating properties from the given control to this control. Used when instantiating children.
        public void InheritProperties(UIControl from)
        {
            foreach (var fromProp in from.Properties)
            {
                if (Properties.SingleOrDefault(x => x.Name.Equals(fromProp.Name)) is { } myProp && fromProp.Inherited)
                {
                    myProp.Value = fromProp.Value;
                }
            }
        }

        public UIControl(GameInput input)
        {
            Input = input;
            Children = new();

            Properties = GetType()
                .GetRuntimeFields()
                .Where(p => p.FieldType.GetInterface(nameof(IUIControlProperty)) != null)
                .Select(p => (IUIControlProperty)p.GetValue(this))
                .ToList();
            var registerInvalidationEvents = typeof(UIControl)
                .GetMethod(nameof(RegisterInvalidationEvents), BindingFlags.Instance | BindingFlags.NonPublic);

            Children.CollectionChanged += (sender, e) =>
            {
                if (e.OldItems != null)
                    foreach (UIControl i in e.OldItems)
                        i.Invalidated -= PropagateInvalidation;
                if (e.NewItems != null)
                    foreach (UIControl i in e.NewItems)
                        i.Invalidated += PropagateInvalidation;
                Invalidate();
                void PropagateInvalidation(UIControl obj)
                {
                    Invalidate();
                }
            };

            Size.ValueChanged += (_, __) =>
            {
                _target?.Dispose();
                _target = null;
                if (Size.V != Coord.Zero && Scale.V != Vec.Zero)
                {
                    _target = new RenderTexture((uint)(Size.V.X * Scale.V.X), (uint)(Size.V.Y * Scale.V.Y));
                }
                Invalidate();
            };

            foreach (var prop in Properties)
            {
                prop.SetOwner(this);
                if (prop.Invalidating)
                {
                    registerInvalidationEvents.MakeGenericMethod(prop.PropertyType)
                        .Invoke(this, new object[] { prop });
                }
            }
        }


        private void RegisterInvalidationEvents<T>(UIControlProperty<T> prop)
        {
            prop.ValueChanged += (_, __) => Invalidate();
        }

        public virtual IEnumerable<UIControl> Contains(Coord point)
        {
            if (point.X >= BorderRenderPos.X * Scale.V.X
                && point.X < BorderRenderPos.X * Scale.V.X + BorderRenderSize.X * Scale.V.X
                && point.Y >= BorderRenderPos.Y * Scale.V.Y
                && point.Y < BorderRenderPos.Y * Scale.V.Y + BorderRenderSize.Y * Scale.V.Y)
            {
                foreach (var child in Children
                    .Where(c => !c.IsHidden))
                {
                    foreach (var s in child.Contains(point))
                    {
                        yield return s;
                    }
                }
                yield return this;
            }
        }

        public bool Click(Coord mousePos, Mouse.Button button)
        {
            var preventDefault = Clicked?.Invoke(this, mousePos, button) ?? false;
            return preventDefault || OnClicked(mousePos, button);
        }

        protected bool TrackMouse(Coord mousePos, out UIControl clickedControl, out Mouse.Button clickedButton)
        {
            clickedButton = default;
            clickedControl = default;
            var wasInside = Contains(TrackedMousePosition).Any();
            var isInside = Contains(mousePos).Any();
            var leftClick = Input.IsButtonReleased(Mouse.Button.Left);
            var rightClick = Input.IsButtonReleased(Mouse.Button.Right);
            var click = leftClick || rightClick;
            var preventDefault = false;
            if (!wasInside && isInside)
            {
                MouseEntered?.Invoke(this, mousePos);
                OnMouseEntered(mousePos);
                Invalidate();
                if (!click)
                {
                    foreach (var child in Children)
                    {
                        child.TrackMouse(mousePos, out _, out _);
                    }
                }
            }
            else if (wasInside && !isInside)
            {
                MouseLeft?.Invoke(this, mousePos);
                OnMouseLeft(mousePos);
                Invalidate();
                if (!click)
                {
                    foreach (var child in Children)
                    {
                        child.TrackMouse(mousePos, out _, out _);
                    }
                }
            }
            else if (wasInside && isInside)
            {
                MouseMoved?.Invoke(this, mousePos);
                OnMouseMoved(mousePos);
                if (!click)
                {
                    foreach (var child in Children)
                    {
                        child.TrackMouse(mousePos, out _, out _);
                    }
                }
            }
            if (isInside && click)
            {
                foreach (var con in Children
                    .SelectMany(child => child.Contains(mousePos)
                        .Where(con => con.IsInteractive.V)))
                {
                    clickedControl = con;
                    break;
                }
                clickedControl ??= this;
                clickedButton = leftClick ? Mouse.Button.Left : Mouse.Button.Right;
                preventDefault = clickedControl.Click(mousePos, clickedButton);
            }
            TrackedMousePosition = mousePos;
            return preventDefault;
        }

        public virtual void Update()
        {
            foreach (var child in Children)
            {
                child.Update();
            }
        }

        protected virtual void DrawBackground(RenderTarget target, RenderStates states)
        {
            var rect = new RectangleShape((BorderRenderSize - new Vec(OutlineThickness.V, OutlineThickness.V) * 2).ToVector2f())
            {
                Position = BorderRenderPos.ToVector2f(),
                FillColor = Background,
                OutlineThickness = OutlineThickness,
                OutlineColor = OutlineColor
            };
            target.Draw(rect, states);
        }

        protected virtual void Render(RenderTarget target, RenderStates states)
        {
            DrawBackground(target, states);
            foreach (var child in Children.OrderByDescending(x => x.ZOrder.V).ThenByDescending(x => x.IsActive.V ? 0 : 1))
            {
                child._isDirty = true;
                child.Draw(target, states);
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            if (_target is null) return;
            if (_isDirty)
            {
                _isDirty = false;
                _target.Clear(Background.V);
                var innerStates = RenderStates.Default;
                innerStates.Transform.Translate((Position.V * Coord.NegativeOne).ToVector2f());
                Render(_target, innerStates);
                _target.Display();
            }
            using var sprite = new Sprite(_target.Texture) { Position = Position.V };
            target.Draw(sprite, states);
        }

        public virtual void Dispose() { }
    }
}
